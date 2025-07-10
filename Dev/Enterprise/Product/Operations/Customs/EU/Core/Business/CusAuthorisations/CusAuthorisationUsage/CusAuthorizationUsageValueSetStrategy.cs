using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class CusAuthorizationUsageValueSetStrategy : IValueSetStrategy
	{
		public CusAuthorizationUsageValueSetStrategy(CusAuthorizationUsage header)
		{
			Header = header;
		}

		protected readonly CusAuthorizationUsage Header;

		public void ValueSet(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			ValueSetCore(valueThatHasChanged, oldValue);
		}

		protected virtual void ValueSetCore(ZPropertyInfo valueThatHasChanged, IZType oldValue)
		{
			switch (valueThatHasChanged.Name)
			{
				case CusAuthorizationUsage.Schema.AGC_Code:
				case CusAuthorizationUsage.Schema.AGC_OH_Owner:
					ChangeAGC_CodeOrAGC_OH_Owner();
					break;
				case CusAuthorizationUsage.Schema.AGC_CPH_Authorization:
					ChangeInAGC_CPH_Authorization();
					break;
				case CusAuthorizationUsage.Schema.AGC_Number:
					ChangeInAGC_Number();
					break;
			}
		}

		void ChangeInAGC_CPH_Authorization()
		{
			if (Header.AuthorisationHeader != null)
			{
				Header.AGC_Number = ZString.Empty;
			}
		}

		void ChangeInAGC_Number()
		{
			if (!Header.AGC_Number.IsEmpty)
			{
				Header.AGC_CPH_Authorization = ZGuid.Empty;
			}
		}

		void ChangeAGC_CodeOrAGC_OH_Owner()
		{
			if (!Header.AGC_Code.IsEmpty && !Header.AGC_OH_Owner.IsEmpty)
			{
				Header.Instruction?.CreateSupportingDocsOnInvoiceLinesFromAuthorisationUsage(Header);
			}
		}
	}
}

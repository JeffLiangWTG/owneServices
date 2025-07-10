using System.Linq;
using Enterprise.Customs.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class TariffDetachValidation : CusCodeDataValidation
	{
		public TariffDetachValidation(TariffDetach parent)
			: base(parent)
		{
		}

		public new TariffDetach Parent => (TariffDetach)base.Parent;

		protected override void CheckCY_Code()
		{
			base.CheckCY_Code();

			var targetInfo = Parent.CY_CodeInfo;
			var referenceNumber = Parent.CY_Code;
			var parent = Parent.TariffDetachParent;

			if (!referenceNumber.IsEmpty && parent != null)
			{
				var tariffDetachCollection = parent.TariffDetachs.Cast<TariffDetach>();

				var hasDuplicates = tariffDetachCollection.Any(x => x.CY_Code == referenceNumber && x.PK != Parent.PK);
				if (hasDuplicates)
				{
					targetInfo.AddMessageError(Res.GetString("73b0697b-cb5c-4870-814d-63344eb6f2ca", "This Tariff Detach code already exists in this invoice line"));
				}
			}
		}

		protected override void CheckCY_CodeList() { }
	}
}

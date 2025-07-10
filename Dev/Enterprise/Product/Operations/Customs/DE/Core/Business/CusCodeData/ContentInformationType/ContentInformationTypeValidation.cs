using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business
{
	public class ContentInformationTypeValidation : Customs.Business.CusCodeDataValidation
	{
		public ContentInformationTypeValidation(ContentInformationType parent)
			: base(parent)
		{
		}

		protected new ContentInformationType Parent => (ContentInformationType)base.Parent;

		protected override void CheckCY_Code()
		{
			var parent = Parent;
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.CY_CodeInfo, parent.Lookups.CY_CodeList);

			var code = parent.CY_Code;
			if (!code.IsEmpty
				&& parent.Parent is JobComInvoiceLine invoiceLine
				&& invoiceLine.ContentInformationTypes.Cast<ContentInformationType>().Any(x => x.CY_Code == code && x.PK != parent.PK))
			{
				parent.CY_CodeInfo.AddMessageError(Res.GetString("A9DD156E-BEBC-4E3F-825D-18D8CE1EE723", "Content Information Type should not be duplicated."));
			}
		}

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();
			if (!Parent.CY_Code.IsEmpty)
			{
				var result = ZDecimal.ParseSafe(Parent.CY_Data, ZDecimal.Zero);
				if (!result.IsInRange(0.01m, 100.00m))
				{
					Parent.CY_DataInfo.AddMessageError(Res.GetString("278B096F-C3C3-4D7C-83EF-63450E7A18A3", "The value should be between 0.01 and 100.00."));
				}
			}
		}
	}
}

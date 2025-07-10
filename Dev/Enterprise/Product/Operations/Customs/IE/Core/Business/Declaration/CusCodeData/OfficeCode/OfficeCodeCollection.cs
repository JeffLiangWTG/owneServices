using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class OfficeCodeCollection : EuOfficeCodeCollection
	{
		public OfficeCodeCollection(JobDeclaration master)
			: base(master)
		{
		}

		public OfficeCode GetPresentationOffice() => GetOffice(EuOfficeCodesTypes.Codes.OfficeOfPresentation);
		public OfficeCode GetSupervisingOffice() => GetOffice(EuOfficeCodesTypes.Codes.SupervisingOffice);
		public OfficeCode GetOfficeOfExit() => GetOffice(EuOfficeCodesTypes.Codes.OfficeOfExit);
		public OfficeCode GetOfficeOfDischarge() => GetOffice(EuOfficeCodesTypes.Codes.OfficeOfDischarge);

		public override Type GetTypeOfElementsFromPK(ZGuid pk) => typeof(OfficeCode);

		public new OfficeCode AddNew() => (OfficeCode)base.AddNew();
		public new OfficeCode AddNew(ZString code) => (OfficeCode)base.AddNew(code);
		public new OfficeCode AddNew(ZString code, ZString data) => (OfficeCode)base.AddNew(code, data);

		public new OfficeCode this[int index] => (OfficeCode)base[index];

		public OfficeCode GetOffice(string code) => this.Cast<OfficeCode>().Where(x => x.CY_Code == code).OrderBy(x => x.CY_SystemCreateTimeUtc).FirstOrDefault();
	}
}

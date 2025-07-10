using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.H7.Business
{
	public class PreviousDocument : EU.H7.Business.PreviousDocument
	{
		public PreviousDocument(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new PreviousDocumentLookups Lookups => (PreviousDocumentLookups)base.Lookups;

		protected override CusSupportingInfoLookups GetNewLookups() => new PreviousDocumentLookups(this);

		protected override CusSupportingInfoValidation GetNewValidation()
		{
			return new PreviousDocumentValidation(this);
		}

		protected override ZString GetPreviousDocumentDescription() => (Lookups?.CodeList is CodeDescriptionPairList codeDescriptionPairList) ? codeDescriptionPairList.GetDescriptionFromCode(CSI_Code) : string.Empty;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_SubType = "Z";
		}
	}
}

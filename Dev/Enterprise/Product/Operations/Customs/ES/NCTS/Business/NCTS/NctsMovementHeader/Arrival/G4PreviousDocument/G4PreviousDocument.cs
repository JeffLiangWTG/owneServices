using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class G4PreviousDocument : EU.Business.Declaration.MultiLineAddInfos.PreviousDocument
	{
		public G4PreviousDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override CusSupportingInfoValidation GetNewValidation() => new G4PreviousDocumentValidation(this);

		const string G4SummaryDeclarationCode = "G4";

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Code = G4SummaryDeclarationCode;
		}

		protected override ZString HumanReadableNameCore => Res.GetString("F97EF17A-7DCF-4C29-93FA-EEE5A3CB1F35", "G4 Previous Document");
	}
}

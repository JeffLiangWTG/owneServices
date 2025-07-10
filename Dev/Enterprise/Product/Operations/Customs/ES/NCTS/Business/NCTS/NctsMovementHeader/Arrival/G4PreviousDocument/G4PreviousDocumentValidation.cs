namespace Enterprise.Customs.ES.NCTS.Business
{
	public class G4PreviousDocumentValidation : EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentValidation
	{
		public G4PreviousDocumentValidation(G4PreviousDocument parent) : base(parent)
		{
		}

		protected override void CheckCSI_SubType() { }

		protected override void CheckCSI_Code() { }
	}
}

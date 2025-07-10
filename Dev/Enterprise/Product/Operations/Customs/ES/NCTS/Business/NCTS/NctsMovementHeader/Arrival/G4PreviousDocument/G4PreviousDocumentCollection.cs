namespace Enterprise.Customs.ES.NCTS.Business
{
	public class G4PreviousDocumentCollection : EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection
	{
		public G4PreviousDocumentCollection(NctsArrivalMovementHeader parent) : base(parent)
		{
			MaxCountValidationEnable(MaxAllowedG4PreviousDocuments);
		}

		const int MaxAllowedG4PreviousDocuments = 99;

		public new G4PreviousDocument this[int i] => (G4PreviousDocument)base[i];

		public new G4PreviousDocument AddNew() => (G4PreviousDocument)base.AddNew();
	}
}

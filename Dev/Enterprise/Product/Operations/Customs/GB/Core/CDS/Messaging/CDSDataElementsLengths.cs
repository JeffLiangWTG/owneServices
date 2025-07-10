namespace Enterprise.Customs.GB.CDS.Messaging
{
	public static class CDSDataElementsLengths
	{
		//IAdditionalDocument
		public const int AdditionalDocumentIdMaxLength = 35;
		public const int AdditionalDocumentNameMaxLength = 35;
		public const int AdditionalDocumentSubmitterNameMaxLength = 70;

		//IDecAdditionalDocument

		//IPackaging
		public const int PackagingMarksNumbersIDMaxLength = 512;

		//IPreviousDocument
		public const int PreviousDocumentIDMaxLength = 55;

		//ITransportEquipment
		public const int TransportEquipmentContainerNoMaxLength = 17;

		//ITransportMeans
		public const int TransportMeansIdentificationIDMaxLength = 27;
	}
}

namespace Enterprise.Services.OperationalActions.Business
{
	class DocumentProcessorFactory
	{
		public BaseDocumentProcessor GetProcessor(OperationalActionRunner runner)
		{
			if (runner.DeliverDocumentsInOneEmail)
			{
				return new DeliverDocumentsInOneEmailProcessor(runner);
			}

			return new DeliverDocumentsProcessor(runner);
		}
	}
}

namespace Enterprise.DocumentEngine.GUI
{
	public class DocumentsMenuItem
	{
		public DocumentsMenuItem(DocumentCommand documentCommand, string contentType)
		{
			DocumentCommand = documentCommand;
			Name = documentCommand.SU_MenuNameMultilingual;
			ContentType = contentType;
		}

		public DocumentsMenuItem(DocumentCommand documentCommand, string contentType, string name)
		{
			DocumentCommand = documentCommand;
			Name = name;
			ContentType = contentType;
		}

		public DocumentCommand DocumentCommand { get; set; }
		public string Name { get; set; }
		public string ContentType { get; set; }
	}
}

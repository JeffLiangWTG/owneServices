using CargoWise.BuildTools;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

#region SuppressResourceStringsCheckRegion

namespace Enterprise.DocumentEngine.Build
{
	public sealed class TemplateSerializationTask : Task
	{
		public override bool Execute()
		{
			BuildConstants.LocalEnterprisePath = BaseDirectory;

			bool result = TemplateSerializationHelper.Execute(BaseDirectory, DocumentsXmlPath, DocumentsWithBlobsXmlPath, out var messages);

			foreach (LogMessage message in messages)
			{
				switch (message.Type)
				{
					case 'M': Log.LogMessage("TemplateSerializationTask: " + message.Text); break;
					case 'E': Log.LogError("TemplateSerializationTask: " + message.Text); break;
					case 'X': Log.LogErrorFromException(message.Exception); break;
					case 'W': Log.LogWarning("TemplateSerializationTask: " + message.Text); break;
				}
			}

			return result;
		}

		#region MSBuild Properties

		[Required]
		public string BaseDirectory
		{
			get;
			set;
		}

		[Required]
		public string DocumentsXmlPath
		{
			get { return fDocumentsXmlPath; }
			set { fDocumentsXmlPath = value; }
		}
		string fDocumentsXmlPath;

		[Required]
		public string DocumentsWithBlobsXmlPath
		{
			get { return fDocumentsWithBlobsXmlPath; }
			set { fDocumentsWithBlobsXmlPath = value; }
		}
		string fDocumentsWithBlobsXmlPath;

		#endregion
	}
}

#endregion

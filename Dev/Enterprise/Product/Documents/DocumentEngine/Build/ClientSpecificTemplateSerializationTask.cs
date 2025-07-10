using System.Collections.Generic;
using CargoWise.BuildTools;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

#region SuppressResourceStringsCheckRegion

namespace Enterprise.DocumentEngine.Build
{
	public class ClientSpecificTemplateSerializationTask : Task
	{
		public override bool Execute()
		{
			BuildConstants.LocalEnterprisePath = BaseDirectory;
			List<LogMessage> messages;
			bool result = TemplateSerializationHelper.Execute(BaseDirectory, DocumentsXmlPath, out messages);

			foreach (LogMessage message in messages)
			{
				switch (message.Type)
				{
					case 'M': Log.LogMessage("ClientSpecificTemplateSerializationTask: " + message.Text); break;
					case 'E': Log.LogError("ClientSpecificTemplateSerializationTask: " + message.Text); break;
					case 'X': Log.LogErrorFromException(message.Exception); break;
					case 'W': Log.LogWarning("ClientSpecificTemplateSerializationTask: " + message.Text); break;
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
		public string DocumentsXmlPath { get; set; }

		#endregion
	}
}

#endregion
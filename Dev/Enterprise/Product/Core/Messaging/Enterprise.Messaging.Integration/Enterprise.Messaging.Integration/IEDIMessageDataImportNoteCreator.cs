using System;
using System.IO;

namespace Enterprise.Messaging.Integration
{
	public interface IEDIMessageDataImportNoteCreator
	{
		void AddNew(IEDIMessage message, Action<Stream> getNoteTextIntoStream);
	}
}

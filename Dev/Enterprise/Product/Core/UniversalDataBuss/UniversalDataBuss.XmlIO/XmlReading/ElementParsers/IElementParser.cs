using System;
using CargoWise.IO;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.XmlIO.XmlReading.ElementReaders
{
	public interface IElementParser : IElementNavigator
	{
		bool ShouldWarnAboutUnrecognizedAttribute { get; }
		void ParseOutContent(ICurrentElement currentElement, IDataObject targetDataObject);
	}

	public interface IXmlDomObject : IDisposable
	{
		int OpenLineNumber { get; }
	}

	public interface IXmlComment : IXmlDomObject
	{
		SubStreamableStream CommentText { get; }
	}

	public interface ICurrentElement : IXmlDomObject
	{
		string Name { get; }
		SubStreamableStream Body { get; }
	}

	public interface IMalformedXmlElement : IXmlDomObject
	{
		SubStreamableStream Body { get; }
	}
}

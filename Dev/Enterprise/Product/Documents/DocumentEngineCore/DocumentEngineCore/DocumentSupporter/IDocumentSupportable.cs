using System;

namespace Enterprise.DocumentEngineCore.DocumentSupport
{
	public interface IDocumentSupportable
	{
		/// <summary>
		/// Extended properties for supporting document customisation and rendering.
		/// </summary>
		DocumentSupporter DocumentSupporter { get; }

		/// <summary>
		/// Table name for the business object. Return string.Empty if there is no table.
		/// </summary>
		string TableName { get; }
	}

	public interface IDocumentSupportableOverrideType
	{
		Type DocumentSupportableType { get; }
	}
}

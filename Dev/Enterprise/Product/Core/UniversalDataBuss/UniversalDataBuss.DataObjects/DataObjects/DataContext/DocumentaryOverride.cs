using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public sealed class DocumentaryOverride : IDataObject, IDocumentaryOverride
	{
		[MaxLength(256)]
		public ZString? DocumentName { get; set; }
		public CodeDescriptionPair Purpose { get; set; }
		public ZBool? IsSystemDefined { get; set; }
		public ZInt? DataVersion { get; set; }
		public ZInt? SubmissionVersion { get; set; }

		#region IDocumentaryOverride members

		ICodeDescriptionDataObject IDocumentaryOverride.Purpose
		{
			get { return Purpose; }
		}

		#endregion
	}
}

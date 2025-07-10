using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.TestDataObjects
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public sealed class UberDocumentaryOverride : IDataObject, IDocumentaryOverride
	{
		[MaxLength(256)]
		public ZString? DocumentName { get; set; }
		public UberCodeDescriptionPair Purpose { get; set; }
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

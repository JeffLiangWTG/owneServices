using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CTORECAmendmentGenerator : CMRAmendmentGenerator
	{
		public CTORECAmendmentGenerator(ExportCustomsManifestLines line)
			: base(line)
		{
			this.line = line;
		}
		readonly ExportCustomsManifestLines line;

		protected internal override CMRMessageBuilder GetBuilder(BusinessObject bizo) => new CTORECMessageBuilder(line);

		protected internal override EDIMessageCollection GetMesssageCollection(BusinessObject bizo) => line.Messages;

		protected override ZPropertyInfo[] UniqueIdentifierInfos => new ZPropertyInfo[] { line.EL_UserReferenceNumInfo };
	}
}

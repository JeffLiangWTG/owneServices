using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CTOREMAmendmentGenerator : CMRAmendmentGenerator
	{
		public CTOREMAmendmentGenerator(ExportCustomsManifestLines line)
			: base(line)
		{
			this.line = line;
		}
		readonly ExportCustomsManifestLines line;

		protected internal override CMRMessageBuilder GetBuilder(CargoWise.EntityFramework.BusinessObject bizo) => new CTOREMMessageBuilder(line);

		protected internal override EDIMessageCollection GetMesssageCollection(CargoWise.EntityFramework.BusinessObject bizo) => line.Messages;

		protected override CargoWise.EntityFramework.ZPropertyInfo[] UniqueIdentifierInfos => new CargoWise.EntityFramework.ZPropertyInfo[] { line.EL_UserReferenceNumInfo };
	}
}

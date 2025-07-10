using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentParsing;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class UpgradeRequestCollectionContainerParser : DocumentParser<UpgradeRequestCollectionContainer>
	{
		public UpgradeRequestCollectionContainerParser(BusinessObjectFactory factory)
			: base(factory)
		{ }

		protected override Type TypeOfWrapper
		{
			get { return typeof(DocUpgradeRequestCollectionContainer); }
		}
	}
}

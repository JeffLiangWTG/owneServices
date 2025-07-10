using System.ComponentModel;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class AsycudaManifestQueryMessageSendingObject : AutoAsycudaManifestQueryMessageSendingObject
	{
		public AsycudaManifestQueryMessageSendingObject(AsycudaManifestHeader header)
			: base(header.Factory)
		{
			Header = Argument.NotNull(header, nameof(header));
		}

		public readonly AsycudaManifestHeader Header;

		public AsycudaManifestQueryMessageSendingObjectLookups Lookups => lookups ??= new AsycudaManifestQueryMessageSendingObjectLookups(this);
		AsycudaManifestQueryMessageSendingObjectLookups lookups;

		[ReadOnly(true)]
		public override ZString MessageType { get => base.MessageType; set => base.MessageType = value; }

		[ReadOnly(true)]
		public override ZString MessageSubType { get => base.MessageSubType; set => base.MessageSubType = value; }

		public override ZString MessageSubTypeDescription => Lookups.MessageSubTypes.GetDescriptionFromCode(MessageSubType);

		[ReadOnly(true)]
		public override ZString ManifestNumber { get => base.ManifestNumber; set => base.ManifestNumber = value; }

		[ReadOnly(true)]
		public override ZString ParentDealNumber { get => base.ParentDealNumber; set => base.ParentDealNumber = value; }
	}
}

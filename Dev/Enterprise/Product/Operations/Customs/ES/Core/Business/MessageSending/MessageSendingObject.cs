using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business
{
	public class MessageSendingObject : GenericMessageSendingObject
	{
		public MessageSendingObject(JobDeclaration declaration, GlbStaff broker) : base(broker, declaration?.JE_CustomsProfile ?? ZString.Empty)
		{
			Declaration = Argument.NotNull(declaration, nameof(declaration));
		}
		public JobDeclaration Declaration { get; }
	}
}

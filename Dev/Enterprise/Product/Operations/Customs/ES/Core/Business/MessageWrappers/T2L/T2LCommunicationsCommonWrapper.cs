using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class T2LCommunicationsCommonWrapper : IT2LCommunicationsCommon
	{
		public T2LCommunicationsCommonWrapper(JobDeclaration declaration)
		{
			jobDeclaration = Argument.NotNull(declaration, nameof(declaration));
		}
		readonly JobDeclaration jobDeclaration;

		public ZString DeclarationEmail => jobDeclaration.DeclEmailAddr;

		public ZString OtherEmail => jobDeclaration.ZG_OtherEmailAddr;

		public ZBool GreenCircuitIndicator => true;
	}
}

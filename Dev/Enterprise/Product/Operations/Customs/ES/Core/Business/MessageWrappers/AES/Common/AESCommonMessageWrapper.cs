using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;
using JobDeclaration = Enterprise.Customs.ES.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class AESCommonMessageWrapper : IAESCommonMessage
	{
		public AESCommonMessageWrapper(CusEntryHeader entryHeader)
		{
			this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
			declaration = Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
		}

		readonly CusEntryHeader entryHeader;
		readonly JobDeclaration declaration;

		public ZString Sender
		{
			get
			{
				var representativeId = OrgHeaderExtension.GetIDCode(declaration.Representative?.Header);
				var representativeOrDeclarantId = representativeId.IsEmpty ? OrgHeaderExtension.GetIDCode(declaration?.DeclarantOrgAddress?.Header) : representativeId;
				return representativeOrDeclarantId.IsEmpty ? declaration.Supplier.GetIDCode() : representativeOrDeclarantId;
			}
		}

		public ZString MessageIdentification => entryHeader.CH_BGMReference;
	}
}

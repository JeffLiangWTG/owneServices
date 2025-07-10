using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Schema;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using WTG.Serialization.DataScience.Audit.ObjectModel;
using ZClientEDI.Business.Registry;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Core
{
	public class StmDataSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Core>
	{
		public override int DataSchemaVersion => 1;

		public override string Code => SubscriberCodes.StmDataSubscriberCode;

		public override ITableSchema Table { get; } = StmDataSchema.Instance;

		public override bool ShouldSendAuditRow(AuditRow auditRow)
		{
			var registryItem = GetRegistryItemNameFromStmDataAuditRow(auditRow);
			if (registryItem == null || IsRegistryItemSensitiveByMetadata(registryItem) || IsRegistryItemBlacklisted(registryItem))
			{
				return false;
			}
			return true;
		}

		public static IRegistryItem GetRegistryItemNameFromStmDataAuditRow
			(AuditRow auditRow)
		{
			var nameColumn = StmDataSchema.SD_Name.Name;
			object registryName = null;
			if (!auditRow.ColumnValues.TryGetValue(nameColumn, out registryName) || registryName is null)
			{
				return null;
			}
			var registryItemName = registryName.ToString();
			var registryItemObj = EDIDataRegistry.Instance.FindByName(registryItemName);
			return registryItemObj;
		}

		public static bool IsRegistryItemSensitiveByMetadata(IRegistryItem registryItem)
		{
			return registryItem != null
				&& ((registryItem is Ms365OAuth2TokenRegistryDataType) ||
				(registryItem is BinaryKeyRegistryDataType) ||
				(registryItem is SecureStringRegistryDataType) ||
				(registryItem is SecurityIdentifierRegistryDataType) ||
				(registryItem is StringRegistryItem && (
					registryItem.DataType is Ms365OAuth2TokenRegistryDataType ||
					registryItem.DataType is BinaryKeyRegistryDataType ||
					registryItem.DataType is SecureStringRegistryDataType ||
					registryItem.DataType is SecurityIdentifierRegistryDataType)) ||

				(registryItem.EditorInfo is TextRegistryEditorInfo editorInfo
				&& editorInfo.EditorType == TextEditorType.Password) ||
				(registryItem.EditorInfo is AuthenticationRegistryItemEditorInfo) ||
				(registryItem.EditorInfo is LoginPasswordPairListEditorInfo) ||
				(registryItem.EditorInfo is CWSupportLoginTokenPrivateKeyEditorInfo) ||
				(registryItem.EditorInfo is OIDCConfigRegistryEditorInfo) ||
				(registryItem.EditorInfo is Ms365OAuth2TokenRegistryEditorInfo) ||
				(registryItem.EditorInfo is FileUpLoaderX509CertificateRegistryEditorInfo));
		}

		public static bool IsRegistryItemBlacklisted(IRegistryItem registryItem) => blacklistPatterns.Any(pattern => registryItem != null && pattern.IsMatch(registryItem.Name));

		public override IEnumerable<SchemaColumn> BizObjColumns => new SchemaColumn[]
		{
			StmDataSchema.PK,
			StmDataSchema.SD_BinaryValue,
			StmDataSchema.SD_DepartmentGuid,
			StmDataSchema.SD_GuidValue,
			StmDataSchema.SD_IsCancelled,
			StmDataSchema.SD_IsLogged,
			StmDataSchema.SD_Name,
			StmDataSchema.SD_Owner,
			StmDataSchema.SD_PreserveTestValue,
			StmDataSchema.SD_SystemCreateTimeUtc,
			StmDataSchema.SD_SystemCreateUser,
			StmDataSchema.SD_SystemLastEditTimeUtc,
			StmDataSchema.SD_SystemLastEditUser,
			StmDataSchema.SD_Type
		};

		static readonly ConcurrentBag<Regex> blacklistPatterns = new()
		{
			new ("user(id|name|account)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
			new ("pass(word|code|phrase)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
			new ("(access|api|private|public|secret)(key|token)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
			new ("log(in|on)$", RegexOptions.IgnoreCase | RegexOptions.Compiled),
			new ("secret$", RegexOptions.IgnoreCase | RegexOptions.Compiled),
			new ("authentication|authorization", RegexOptions.IgnoreCase),
			new ("auth$", RegexOptions.IgnoreCase),
			new ("credential", RegexOptions.IgnoreCase),
			new ("ClientID|TenantID|OpenID", RegexOptions.IgnoreCase),
			new ("EmailAddressBlockList", RegexOptions.IgnoreCase),
			new ("ConnectionString", RegexOptions.IgnoreCase)
		};
	}
}

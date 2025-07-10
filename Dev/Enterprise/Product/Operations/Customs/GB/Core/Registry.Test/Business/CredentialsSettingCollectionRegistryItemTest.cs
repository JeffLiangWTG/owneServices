using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;
using static Enterprise.Customs.GB.Registry.CredentialsSettingCollectionRegistryItem;

namespace Enterprise.Customs.GB.Registry.Business.Testing
{
	[TestedType(typeof(CredentialsSettingCollectionRegistryItem))]
	public class CredentialsSettingCollectionRegistryItemTest : StronglyTypedRegistryItemTestCase<CredentialsSettingCollection>
	{
		protected override StronglyTypedRegistryItem<CredentialsSettingCollection, CredentialsSettingCollection> GetNewRegistryItem()
		{
			return new CredentialsSettingCollectionRegistryItem("", (NoResString)"", "", "", RegistryStorageFlags.System);
		}
	}

	[TestedType(typeof(CredentialsSettingRegistryDataType))]
	class CredentialsSettingRegistryDataTypeNonPersistentTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CredentialsSettingRegistryDataType>
	{
		protected override string ExpectedEditorName => "CredentialsRegistryItemEditor";

		protected override CredentialsSettingRegistryDataType GetNewDataType()
		{
			return new CredentialsSettingRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var branchCode = "FRD";

			var badge = new BadgeCodeSetting();
			badge.BadgeCode = branchCode;
			badge.CSPCode = GatewayList.Codes.CCSUKviaNTMsgGW;
			var badges = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(System.Guid.Empty, MasterFiles.Business.GlbBranch.CurrentBranch.PK.ToGuid(), System.Guid.Empty);
			badges.Add(badge);
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(System.Guid.Empty, MasterFiles.Business.GlbBranch.CurrentBranch.PK.ToGuid(), System.Guid.Empty, badges);

			var collection = new CredentialsSettingCollection();
			var credentialSetting = collection.AddNew();
			credentialSetting.Password = "abc";
			credentialSetting.BadgeCode = branchCode;

			var collection2 = new CredentialsSettingCollection();
			var credentialSetting2 = collection2.AddNew();
			credentialSetting2.Password = "abcd";
			credentialSetting2.BadgeCode = branchCode;

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, new CredentialsSettingRegistryDataType().Serialise(collection)),
				new ValidSampleAndBinaryValueInDB(collection2, new CredentialsSettingRegistryDataType().Serialise(collection2))
			};
		}
	}
}

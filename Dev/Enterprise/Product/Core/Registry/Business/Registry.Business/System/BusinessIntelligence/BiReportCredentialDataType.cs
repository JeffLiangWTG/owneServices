using System;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class BiReportCredentialRegistryItem : StronglyTypedRegistryItem<BiReportCredential>
	{
		public BiReportCredentialRegistryItem(ZString name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, BiReportCredential defaultValue)
			: base(new BiReportCredentialRegistryItemImpl(name, category, caption, hint, storage, options, defaultValue))
		{ }
		public void SetValue(BiReportCredential value)
		{
			this.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}

		public void ValidateCore(Guid companyOrOwnerPK, Guid branchPK, Guid departmentPK, BiReportCredential proposedValue)
		{
			CannotSetThisItemWhenPrerequisiteHasNotBeenSet(proposedValue, companyOrOwnerPK, branchPK, departmentPK);
			CannotRemoveThisItemWhenPostrequisiteHasValue(proposedValue, companyOrOwnerPK, branchPK, departmentPK);
		}

		void CannotSetThisItemWhenPrerequisiteHasNotBeenSet(BiReportCredential proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			var prerequisiteHasValue = SystemDataRegistry.Instance.BiDataWarehouseServer.HasBeenSetOrChanged(companyPK, branchPK, departmentPK);
			if (proposedValue != null && !prerequisiteHasValue)
			{
				throw new RegistryValidationException(Res.GetString("8AAC3EFB-F8E3-4E4E-BD5B-EB21318ED7BE", "\"Report User Credentials\" cannot be set if \"Data Warehouse Server\" hasn't been set."));
			}
		}

		void CannotRemoveThisItemWhenPostrequisiteHasValue(BiReportCredential proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			var postrequisiteIsEmpty = SystemDataRegistry.Instance.BiAnalysisServer.IsEmpty(companyPK, branchPK, departmentPK);
			if (proposedValue.IsEmpty() && !postrequisiteIsEmpty)
			{
				throw new RegistryValidationException(Res.GetString("7E70676B-B59A-45B5-A6E4-A18A914FDB29", "\"Report User Credentials\" cannot be removed when \"Analysis Server\" has a value specified."));
			}
		}

		protected override void SetValueCore(Guid companyOrOwnerPK, Guid branchPK, Guid departmentPK, object newValue)
		{
			this.ValidateCore(companyOrOwnerPK, branchPK, departmentPK, (BiReportCredential)newValue);
			base.SetValueCore(companyOrOwnerPK, branchPK, departmentPK, newValue);
		}

		public override bool HasBeenSetOrChanged(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			string savedUsername = Value == null ? null : Value.UserName;
			BiReportCredential changedValue = (BiReportCredential)((IRegistryItemInternals)this).GetProposedValue(companyPK, branchPK, departmentPK);
			string changedUsername = changedValue == null ? null : changedValue.UserName;
			var result = !string.IsNullOrEmpty(changedUsername) || !string.IsNullOrEmpty(savedUsername);
			return result;
		}

		public bool IsEmpty()
		{
			return this.IsEmpty(Guid.Empty, Guid.Empty, Guid.Empty);
		}

		public override bool IsEmpty(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return this.Value.IsEmpty();
		}

#if DEBUG
		public static BiReportCredential BiReportCredentialTestValue()
		{
			return new BiReportCredential
			{
				Domain = "TestDomain",
				UserName = "TestUserName",
				Password = "P$ssword"
			};
		}
#endif

		class BiReportCredentialRegistryItemImpl : RegistryItemImpl
		{
			public BiReportCredentialRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, BiReportCredential defaultValue)
				: base(name, category, caption, hint, new BiReportCredentialRegistryDataType(), storage, options, defaultValue)
			{ }
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.BiReportCredentialRegistryItemEditor, Enterprise.Registry.GUI")]
	public class BiReportCredentialRegistryDataType : NonPersistentBusinessObjectRegistryDataType<BiReportCredential> { }
}

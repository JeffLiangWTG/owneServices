using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class BranchGroupSettings : RegistryBusinessObjectTemplate
	{
		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var clone = new BranchGroupSettings();
			clone.BranchPK = BranchPK;
			clone.GroupNumber = GroupNumber;
			clone.IsParentBranch = IsParentBranch;

			return clone;
		}

		public BusinessObjectCollection ParentCollection
		{
			get { return GetParentCollection(this, typeof(BranchGroupSettingsCollection)); }
		}

		#region Properties

		[List(nameof(Branches))]
		[ResourceStringData("BranchGroupSettings|BranchPK", Caption = "Branches Permitted to Post Together")]
		public ZGuid BranchPK
		{
			get { return branchPK; }
			set
			{
				SetNonPersistentPropertyValue(BranchPKInfo, ref branchPK, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateBranchPK();
				}
			}
		}

		public ZPropertyInfo BranchPKInfo
		{
			get { return GetZPropertyInfo(nameof(BranchPK)); }
		}

		ZGuid branchPK;

		public GlbBranchDependentCollection Branches
		{
			get { return new GlbBranchDependentCollection(new ReadOnlyBusinessObjectFactory()); }
		}

		[ResourceStringData("BranchGroupSettings|GroupNumber", Caption = "Permitted Posting Group")]
		public ZInt GroupNumber
		{
			get { return groupNumber; }
			set
			{
				SetNonPersistentPropertyValue(GroupNumberInfo, ref groupNumber, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateGroupNumber();
				}
			}
		}

		public ZPropertyInfo GroupNumberInfo
		{
			get { return GetZPropertyInfo(nameof(GroupNumber)); }
		}

		ZInt groupNumber;

		[ResourceStringData("BranchGroupSettings|IsParentBranch", Caption = "Transaction Header Branch Fallback")]
		public ZBool IsParentBranch
		{
			get { return isParentBranch; }
			set
			{
				SetNonPersistentPropertyValue(IsParentBranchInfo, ref isParentBranch, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateIsParentBranch();
				}
			}
		}

		public ZPropertyInfo IsParentBranchInfo
		{
			get { return GetZPropertyInfo(nameof(IsParentBranch)); }
		}

		ZBool isParentBranch;

		#endregion

		#region Validation

		public BranchGroupSettingsValidation Validation => validation ?? (validation = new BranchGroupSettingsValidation(this));
		BranchGroupSettingsValidation validation;

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			Validation.ValidateBranchPK();
			Validation.ValidateGroupNumber();
			Validation.ValidateIsParentBranch();
		}

		#endregion

		#region Default Values

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(nameof(BranchPK), BranchPK.ToString());
			writer.WriteElementString(nameof(GroupNumber), GroupNumber.ToString());
			writer.WriteElementString(nameof(IsParentBranch), IsParentBranch.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			BranchPK = new ZGuid(reader.ReadElementString(nameof(BranchPK)));
			GroupNumber = new ZInt(reader.ReadElementString(nameof(GroupNumber)));
			IsParentBranch = new ZBool(reader.ReadElementString(nameof(IsParentBranch)));
		}

		#endregion
	}
}

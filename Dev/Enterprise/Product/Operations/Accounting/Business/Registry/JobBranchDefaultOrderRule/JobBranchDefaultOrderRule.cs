using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.Accounting.Business.Res;

namespace Enterprise.Accounting.Registry.Business
{
	public interface IJobBranchDefaultOrderRule
	{
		ZShort DefaultToBlank { get; }
		ZShort DefaultToBranchRelatedToPortOrWarehouseBranch { get; }
		ZShort DefaultToBranchOfOrganisation { get; }
		ZShort DefaultToLoginUserDefault { get; }
	}

	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class JobBranchDefaultOrderRule : RegistryBusinessObjectTemplate, IJobBranchDefaultOrderRule
	{
		#region Schema

		public abstract class Schema
		{
			public const string DefaultToBlank = "DefaultToBlank";
			public const string DefaultToBranchRelatedToPortOrWarehouseBranch = "DefaultToBranchRelatedToPortOrWarehouseBranch";
			public const string DefaultToBranchOfOrganisation = "DefaultToBranchOfOrganisation";
			public const string DefaultToLoginUserDefault = "DefaultToLoginUserDefault";
		}

		#endregion

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();

			DefaultToBlank = 0;
			DefaultToBranchRelatedToPortOrWarehouseBranch = 1;
			DefaultToBranchOfOrganisation = 2;
			DefaultToLoginUserDefault = 3;
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new JobBranchDefaultOrderRule();
		}

		#region Bound Properties

		#region Default to Blank

		public ZShort DefaultToBlank
		{
			get { return defaultToBlank; }
			set
			{
				SetNonPersistentPropertyValue(DefaultToBlankInfo, ref defaultToBlank, value);
				if (!IsValidationSuspended)
				{
					ValidateDefaultToBlank();
				}
			}
		}

		public ZPropertyInfo DefaultToBlankInfo
		{
			get { return GetZPropertyInfo(Schema.DefaultToBlank, "Default to Blank"); }
		}

		public void ValidateDefaultToBlank()
		{
			DefaultToBlankInfo.ClearAllNotifications();
			CompareValidation.CheckWithinRange(DefaultToBlankInfo, 0, 4);
			ValidateNumberIsUnique(DefaultToBlankInfo);
		}

		ZShort defaultToBlank;

		#endregion

		#region Default to Branch Related to Port / Warehouse Branch

		public ZShort DefaultToBranchRelatedToPortOrWarehouseBranch
		{
			get { return defaultToBranchRelatedToPortOrWarehouseBranch; }
			set
			{
				SetNonPersistentPropertyValue(DefaultToBranchRelatedToPortOrWarehouseBranchInfo, ref defaultToBranchRelatedToPortOrWarehouseBranch, value);
				if (!IsValidationSuspended)
				{
					ValidateDefaultToBranchRelatedToPortOrWarehouseBranch();
				}
			}
		}

		public ZPropertyInfo DefaultToBranchRelatedToPortOrWarehouseBranchInfo
		{
			get { return GetZPropertyInfo(Schema.DefaultToBranchRelatedToPortOrWarehouseBranch, "Default to Branch Related to Port / Warehouse Branch"); }
		}

		public void ValidateDefaultToBranchRelatedToPortOrWarehouseBranch()
		{
			DefaultToBranchRelatedToPortOrWarehouseBranchInfo.ClearAllNotifications();
			CompareValidation.CheckWithinRange(DefaultToBranchRelatedToPortOrWarehouseBranchInfo, 0, 4);
			ValidateNumberIsUnique(DefaultToBranchRelatedToPortOrWarehouseBranchInfo);
		}

		ZShort defaultToBranchRelatedToPortOrWarehouseBranch;

		#endregion

		#region Default to Branch of Organisation

		public ZShort DefaultToBranchOfOrganisation
		{
			get { return defaultToBranchOfOrganisation; }
			set
			{
				SetNonPersistentPropertyValue(DefaultToBranchOfOrganisationInfo, ref defaultToBranchOfOrganisation, value);
				if (!IsValidationSuspended)
				{
					ValidateDefaultToBranchOfOrganisation();
				}
			}
		}

		public ZPropertyInfo DefaultToBranchOfOrganisationInfo
		{
			get { return GetZPropertyInfo(Schema.DefaultToBranchOfOrganisation, "Default to Branch of Organisation"); }
		}

		public void ValidateDefaultToBranchOfOrganisation()
		{
			DefaultToBranchOfOrganisationInfo.ClearAllNotifications();
			CompareValidation.CheckWithinRange(DefaultToBranchOfOrganisationInfo, 0, 4);
			ValidateNumberIsUnique(DefaultToBranchOfOrganisationInfo);
		}

		ZShort defaultToBranchOfOrganisation;

		#endregion

		#region Default to Login User Default

		public ZShort DefaultToLoginUserDefault
		{
			get { return defaultToLoginUserDefault; }
			set
			{
				SetNonPersistentPropertyValue(DefaultToLoginUserDefaultInfo, ref defaultToLoginUserDefault, value);
				if (!IsValidationSuspended)
				{
					ValidateDefaultToLoginUserDefault();
				}
			}
		}

		public ZPropertyInfo DefaultToLoginUserDefaultInfo
		{
			get { return GetZPropertyInfo(Schema.DefaultToLoginUserDefault, "Default to Login User Default"); }
		}

		public void ValidateDefaultToLoginUserDefault()
		{
			DefaultToLoginUserDefaultInfo.ClearAllNotifications();
			CompareValidation.CheckWithinRange(DefaultToLoginUserDefaultInfo, 0, 4);
			ValidateNumberIsUnique(DefaultToLoginUserDefaultInfo);
		}

		ZShort defaultToLoginUserDefault;

		#endregion

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ClearRowNotifications();

			ValidateDefaultToBlank();
			ValidateDefaultToBranchRelatedToPortOrWarehouseBranch();
			ValidateDefaultToBranchOfOrganisation();
			ValidateDefaultToLoginUserDefault();

			ValidateAtLeastOneValueIsGreaterThanZero();
			ValidateNoGapsBetweenValues();
		}

		void ValidateNumberIsUnique(ZPropertyInfo propertyInfo)
		{
			if ((ZShort)propertyInfo.Value != 0)
			{
				CompareValidation.CheckValueIsNotDuplicated(propertyInfo, DefaultToInfos);
			}
		}

		void ValidateAtLeastOneValueIsGreaterThanZero()
		{
			bool foundValidValue = false;

			foreach (ZPropertyInfo propertyInfo in DefaultToInfos)
			{
				if ((ZShort)propertyInfo.Value > 0)
				{
					foundValidValue = true;
					break;
				}
			}

			if (!foundValidValue)
			{
				AddRowError(Res.GetString("3d2bae6f-0a01-4e5c-83f0-49a8203fc025", "Please enter at least one value that is greater than zero."));
			}
		}

		void ValidateNoGapsBetweenValues()
		{
			ZShort largestValue = 0;

			foreach (ZPropertyInfo propertyInfo in DefaultToInfos)
			{
				largestValue = (ZShort)Math.Max((ZShort)propertyInfo.Value, largestValue);
			}

			int count = 0;

			while (largestValue > 0 && count < 3)
			{
				if (!DefaultToInfosContainsValue(largestValue - 1))
				{
					ZShort nextValue = largestValue - 1;

					while (nextValue > 0 && !DefaultToInfosContainsValue(nextValue))
					{
						nextValue--;
					}

					AddRowError(Res.GetString("4f07fceb-fb05-4a64-aa29-875a6fda27bb", "There is a gap between the values {0} and {1}. There should not be any gaps between values.",
						nextValue, largestValue));

					break;
				}

				largestValue--;
				count++;
			}
		}

		bool DefaultToInfosContainsValue(ZShort value)
		{
			foreach (ZPropertyInfo propertyInfo in DefaultToInfos)
			{
				if (propertyInfo.Value.Equals(value))
				{
					return true;
				}
			}

			return false;
		}

		ZPropertyInfo[] DefaultToInfos
		{
			get
			{
				if (fDefaultToInfos == null)
				{
					fDefaultToInfos = new ZPropertyInfo[]
					{
						DefaultToBlankInfo,
						DefaultToBranchRelatedToPortOrWarehouseBranchInfo,
						DefaultToBranchOfOrganisationInfo,
						DefaultToLoginUserDefaultInfo
					};
				}

				return fDefaultToInfos;
			}
		}

		ZPropertyInfo[] fDefaultToInfos;

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.DefaultToBlank, DefaultToBlank.ToString());
			writer.WriteElementString(Schema.DefaultToBranchRelatedToPortOrWarehouseBranch, DefaultToBranchRelatedToPortOrWarehouseBranch.ToString());
			writer.WriteElementString(Schema.DefaultToBranchOfOrganisation, DefaultToBranchOfOrganisation.ToString());
			writer.WriteElementString(Schema.DefaultToLoginUserDefault, DefaultToLoginUserDefault.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			DefaultToBlank = ZShort.Parse(reader.ReadElementString(Schema.DefaultToBlank));
			DefaultToBranchRelatedToPortOrWarehouseBranch = ZShort.Parse(reader.ReadElementString(Schema.DefaultToBranchRelatedToPortOrWarehouseBranch));
			DefaultToBranchOfOrganisation = ZShort.Parse(reader.ReadElementString(Schema.DefaultToBranchOfOrganisation));
			DefaultToLoginUserDefault = ZShort.Parse(reader.ReadElementString(Schema.DefaultToLoginUserDefault));
		}

		#endregion
	}
}
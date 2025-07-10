using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.Organisation.Registry;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.Accounting.Business.Res;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class CreditControlledDocumentsCheckConfiguration : AmountBasedThreeLevelAuthorisationRequirement, IObsoleteValidation, ICreditControlledDocumentsCheckConfiguration
	{
		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : AmountBasedThreeLevelAuthorisationRequirement.Schema
		{
			public const string InvoiceType = "InvoiceType";
			public const string NumberOfDaysOverdue = "NumberOfDaysOverdue";
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CreditControlledDocumentsCheckConfiguration();
		}

		#region Bound Properties

		#region InvoiceType

		[MaxLength(3)]
		[List("InvoiceTypeList")]
		public ZString InvoiceType
		{
			get { return invoiceType; }
			set
			{
				if (value != invoiceType)
				{
					SetNonPersistentPropertyValue(InvoiceTypeInfo, ref invoiceType, value);
					if (!IsValidationSuspended)
					{
						ValidateInvoiceType();
					}
				}
			}
		}

		public ZPropertyInfo InvoiceTypeInfo
		{
			get { return GetZPropertyInfo(Schema.InvoiceType); }
		}

		void ValidateInvoiceType()
		{
			InvoiceTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(InvoiceTypeInfo);
			ListValidation.ErrorIfInvalidCode(InvoiceTypeInfo, InvoiceTypeList);
			if (InvoiceType != CreditControlledDocumentsCheckConfigurationInvoiceTypes.All.Code)
			{
				var collection = ParentCollection != null ?
						ParentCollection.Cast<CreditControlledDocumentsCheckConfiguration>().Where(x => x.InvoiceType == CreditControlledDocumentsCheckConfigurationInvoiceTypes.All.Code) :
						Enumerable.Empty<CreditControlledDocumentsCheckConfiguration>();

				AddErrorIfSameOrConflictingSettingPresent(collection);
			}
			else
			{
				var collection = ParentCollection != null ?
						ParentCollection.Cast<CreditControlledDocumentsCheckConfiguration>().Where(x => x.InvoiceType != CreditControlledDocumentsCheckConfigurationInvoiceTypes.All.Code) :
						Enumerable.Empty<CreditControlledDocumentsCheckConfiguration>();

				AddErrorIfSameOrConflictingSettingPresent(collection);
			}
		}

		void AddErrorIfSameOrConflictingSettingPresent(IEnumerable<AmountBasedMultiLevelAuthorisationRequirement> collection)
		{
			foreach (CreditControlledDocumentsCheckConfiguration item in collection)
			{
				if (Amount == item.Amount && Range == item.Range)
				{
					if (AuthorisationRequirement == item.AuthorisationRequirement && NumberOfDaysOverdue == item.NumberOfDaysOverdue)
					{
						InvoiceTypeInfo.AddError(Res.GetString("bb6d1dd8-54e0-487a-ae9b-4a524e39f2f4", "This setting already exists.", InvoiceType));
					}
					else
					{
						InvoiceTypeInfo.AddError(Res.GetString("7378061d-d00b-45aa-aabc-9de157b8b13e", "This setting conflicts with another setting.", InvoiceType));
					}
				}
			}
		}

		ZString invoiceType;

		#endregion

		#region NumberOfDaysOverdue

		public ZInt NumberOfDaysOverdue
		{
			get { return numberOfDaysOverdue; }
			set
			{
				if (value != numberOfDaysOverdue)
				{
					SetNonPersistentPropertyValue(NumberOfDaysOverdueInfo, ref numberOfDaysOverdue, value);
					if (!IsValidationSuspended)
					{
						ValidateNumberOfDaysOverdue();
					}
				}
			}
		}

		public ZPropertyInfo NumberOfDaysOverdueInfo
		{
			get { return GetZPropertyInfo(Schema.NumberOfDaysOverdue); }
		}

		void ValidateNumberOfDaysOverdue()
		{
			NumberOfDaysOverdueInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(NumberOfDaysOverdueInfo);

			if (!NumberOfDaysOverdueInfo.HasErrors())
			{
				if (NumberOfDaysOverdue < 1 || NumberOfDaysOverdue > 365)
				{
					NumberOfDaysOverdueInfo.AddError(Res.GetString("6a6c17e9-6d5e-4406-814f-267d4b3d8fd2", "Number of Days Overdue must be between 1 and 365"));
				}
			}
			HigherAmountsHigherAuthorisationLevelValidation(NumberOfDaysOverdueInfo);
		}

		ZInt numberOfDaysOverdue;

		protected void HigherAmountsHigherAuthorisationLevelValidation(ZPropertyInfo amountProperty)
		{
			if (!Range.IsEmpty && !AuthorisationRequirement.IsEmpty)
			{
				if (OtherCollectionElements.Any())
				{
					int currentWeight = GetAuthorisationRequirementWeight(AuthorisationRequirement);

					if (Range == RangeCodes.UpTo)
					{
						foreach (CreditControlledDocumentsCheckConfiguration setting in OtherCollectionElements.Where(x => x.Range == RangeCodes.UpTo))
						{
							int settingWeight = GetAuthorisationRequirementWeight(setting.AuthorisationRequirement);
							ZInt settingAmount = (ZInt)setting[amountProperty.Name];
							ZInt currentAmount = (ZInt)amountProperty.Value;
							if ((settingAmount < currentAmount && settingWeight > currentWeight) ||
								(settingAmount > currentAmount && settingWeight < currentWeight))
							{
								amountProperty.AddError(Res.GetString("5ecf432c-cc1c-4cbe-8633-6f686feb374f", "Higher amounts must have '{0}' higher than lower amounts.", amountProperty.HumanReadableName));
								break;
							}
						}
					}
					else if (Range == RangeCodes.Above && OtherCollectionElements.Any())
					{
						ZInt largestUpTo = 0;

						foreach (CreditControlledDocumentsCheckConfiguration setting in OtherCollectionElements)
						{
							ZInt settingAmount = (ZInt)setting[amountProperty.Name];
							if (setting.Range == RangeCodes.UpTo && settingAmount > largestUpTo)
							{
								largestUpTo = settingAmount;
							}
						}
						if ((ZInt)amountProperty.Value != largestUpTo)
						{
							amountProperty.AddError(Res.GetString("371ab989-84a6-468b-a5f6-193dc56aef01", "The Above Line's '{0}' must be {1}.", amountProperty.HumanReadableName, largestUpTo.ToString()));
						}
					}
				}
			}
		}
		#endregion

		#endregion

		#region Lookups

		#region InvoiceType List

		public CodeDescriptionPairList InvoiceTypeList
		{
			get
			{
				if (invoiceTypeList == null)
				{
					invoiceTypeList = new CodeDescriptionPairList();
					invoiceTypeList.Add(CreditControlledDocumentsCheckConfigurationInvoiceTypes.All);
					invoiceTypeList.Add(CreditControlledDocumentsCheckConfigurationInvoiceTypes.DSB);
					invoiceTypeList.Add(CreditControlledDocumentsCheckConfigurationInvoiceTypes.NDB);
				}
				return invoiceTypeList;
			}
		}
		CodeDescriptionPairList invoiceTypeList;

		#endregion

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.InvoiceType, InvoiceType);
			writer.WriteElementString(Schema.NumberOfDaysOverdue, NumberOfDaysOverdue.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			base.ReadElements(reader);
			InvoiceType = reader.ReadElementString(Schema.InvoiceType);
			NumberOfDaysOverdue = ZInt.Parse(reader.ReadElementString(Schema.NumberOfDaysOverdue));
		}

		#endregion

		CreditControlledDocumentsCheckConfigurationCollection ParentCollection
		{
			get { return (CreditControlledDocumentsCheckConfigurationCollection)GetParentCollection(this, typeof(CreditControlledDocumentsCheckConfigurationCollection)); }
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateInvoiceType();
			ValidateNumberOfDaysOverdue();
		}

		protected override IEnumerable<AmountBasedMultiLevelAuthorisationRequirement> GetOtherCollectionElements()
		{
			return ParentCollection != null ?
				ParentCollection.Cast<CreditControlledDocumentsCheckConfiguration>().Where(x => x != this && x.InvoiceType == this.InvoiceType) :
				Enumerable.Empty<CreditControlledDocumentsCheckConfiguration>();
		}
	}
}

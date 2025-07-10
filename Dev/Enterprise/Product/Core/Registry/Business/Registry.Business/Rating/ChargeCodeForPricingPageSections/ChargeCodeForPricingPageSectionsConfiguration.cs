using System;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class ChargeCodeForPricingPageSectionsConfiguration : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string PricingPage = nameof(PricingPage);
			public const string Section = nameof(Section);

			public const int PricingPageMaxLength = 3;
			public const int SectionMaxLength = 3;
		}

		#endregion

		public ChargeCodeForPricingPageSectionsConfiguration()
			: this(null, null)
		{
		}

		public ChargeCodeForPricingPageSectionsConfiguration(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		#region PricingPage

		[MaxLength(Schema.PricingPageMaxLength)]
		[List("PricingPageList")]
		[ResourceStringData("ChargeCodeForPricingPageSectionsConfiguration|PricingPage", Caption = "Pricing Page")]
		public ZString PricingPage
		{
			get { return pricingPage; }
			set
			{
				if (value == string.Empty || value != pricingPage)
				{
					CheckMaximumLength(PricingPageInfo, value);
					SetNonPersistentPropertyValue(PricingPageInfo, ref pricingPage, value);
					ShouldReloadChargeCodeList = true;
					ValidatePricingPage();
					ValidateAllChargeCodesUnderConfiguration();
				}
			}
		}

		ZString pricingPage;

		public ZPropertyInfo PricingPageInfo => GetZPropertyInfo(Schema.PricingPage);

		public void ValidatePricingPage()
		{
			if (!IsValidationSuspended)
			{
				PricingPageInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(PricingPageInfo);
				ListValidation.ErrorIfInvalidCode(PricingPageInfo, PricingPageList);

				ValidateCompositeKey();
			}
		}

		public CodeDescriptionPairList PricingPageList => new () { ChargeCodeForPricingPageSectionsHelper.PricingPage.ForwardingConcise, };

		#endregion

		#region Section

		[MaxLength(Schema.SectionMaxLength)]
		[List("SectionList")]
		[ResourceStringData("ChargeCodeForPricingPageSectionsConfiguration|Section", Caption = "Section")]
		public ZString Section
		{
			get { return section; }
			set
			{
				if (value == string.Empty || value != section)
				{
					CheckMaximumLength(SectionInfo, value);
					SetNonPersistentPropertyValue(SectionInfo, ref section, value);
					ShouldReloadChargeCodeList = true;
					ValidateSection();
					ValidateAllChargeCodesUnderConfiguration();
				}
			}
		}

		ZString section;

		public ZPropertyInfo SectionInfo => GetZPropertyInfo(Schema.Section);

		public void ValidateSection()
		{
			if (!IsValidationSuspended)
			{
				SectionInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(SectionInfo);
				ListValidation.ErrorIfInvalidCode(SectionInfo, SectionList);

				ValidateCompositeKey();
			}
		}

		public CodeDescriptionPairList SectionList =>
			new ()
			{
				ChargeCodeForPricingPageSectionsHelper.Section.OriginPickupCharges,
				ChargeCodeForPricingPageSectionsHelper.Section.DestinationDeliveryCharges,
			};

		#endregion

		#region Charges

		public PricingPageChargeCodeCollection Charges
		{
			get
			{
				if (fCharges == null)
				{
					fCharges = new PricingPageChargeCodeCollection(this, CurrentFallbackLevel, CurrentFactory);
					RegisterEditableChildObject(fCharges);
				}

				return fCharges;
			}
		}

		void CloneChargesFrom(ChargeCodeForPricingPageSectionsConfiguration parentConfiguration, PricingPageChargeCodeCollection existingCharges)
		{
			fCharges = existingCharges.Clone(parentConfiguration, CurrentFallbackLevel, CurrentFactory);
			RegisterEditableChildObject(fCharges);
		}

		ZXmlSerializer ChargeCodeCollectionSerialiser
		{
			get
			{
				if (fChargeCodeCollectionSerialiser == null)
				{
					fChargeCodeCollectionSerialiser = ZXmlSerializer.New(typeof(PricingPageChargeCodeCollection));
				}
				return fChargeCodeCollectionSerialiser;
			}
		}

		PricingPageChargeCodeCollection fCharges;
		ZXmlSerializer fChargeCodeCollectionSerialiser;

		#endregion

		#region ChargeCode List

		public BusinessObjectCollection ChargeCodeList
		{
			get
			{
				if (fChargeCodeList == null || HasFallbackLevelChanged || ShouldReloadChargeCodeList)
				{
					var filter = GetChargeCodesFilter();
					var invalidChargeCodeMessage = GetInvalidChargeCodeMessage();
					if (CurrentFallbackLevel != null)
					{
						fChargeCodeList = (BusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.MasterFiles.Integration.IAccChargeCodeCollection>(), new object[] { CurrentFactory, filter, CurrentFallbackLevel.CompanyPK(false) });
					}
					else
					{
						fChargeCodeList = (BusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.MasterFiles.Integration.IAccChargeCodeCollection>(), new object[] { CurrentFactory });
					}

					fChargeCodeList.SetOverrideNotificationWhenAdditionalFilterNotMet(invalidChargeCodeMessage);
					ShouldReloadChargeCodeList = false;
				}

				return fChargeCodeList;
			}
		}

		bool HasFallbackLevelChanged
		{
			get
			{
				bool result = false;

				if (CurrentChargeCodeListFallbackLevel != CurrentFallbackLevel)
				{
					result = true;
					CurrentChargeCodeListFallbackLevel = CurrentFallbackLevel;
				}

				return result;
			}
		}

		FallbackLevel CurrentChargeCodeListFallbackLevel;

		public bool ShouldReloadChargeCodeList { get; set; }

		ZQuery GetChargeCodesFilter()
		{
			var validGroups = GetSupportedChargeGroups();

			var filter = new ZQuery();
			if (validGroups.Any())
			{
				filter.AddToFilter(JoinCondition.And, AccChargeCodeSchema.AC_RateCalculator, SQLComparisonOperator.NotEqual, ZString.Empty);
				filter.AddToFilter(JoinCondition.And, AccChargeCodeSchema.AC_ChargeGroup, validGroups);
				filter.AddToFilter(JoinCondition.And, AccChargeCodeSchema.AC_IsActive, SQLComparisonOperator.Equal, ZBool.True);
			}

			return filter;
		}

		public readonly string[] OriginPickupChargesChargeCodeGroupList = new[]
		{
			ChargeCodeForPricingPageSectionsHelper.ChargeCodeGroupList.Origin,
			ChargeCodeForPricingPageSectionsHelper.ChargeCodeGroupList.Loading,
			ChargeCodeForPricingPageSectionsHelper.ChargeCodeGroupList.OriginBrokerage,
			ChargeCodeForPricingPageSectionsHelper.ChargeCodeGroupList.OriginBrokerageOnly,
			ChargeCodeForPricingPageSectionsHelper.ChargeCodeGroupList.CustomsDuty,
		};

		public readonly string[] DestinationDeliveryChargesChargeCodeGroupList = new[]
		{
			ChargeCodeForPricingPageSectionsHelper.ChargeCodeGroupList.Destination,
			ChargeCodeForPricingPageSectionsHelper.ChargeCodeGroupList.Unloading,
			ChargeCodeForPricingPageSectionsHelper.ChargeCodeGroupList.Brokerage,
			ChargeCodeForPricingPageSectionsHelper.ChargeCodeGroupList.BrokerageOnly,
			ChargeCodeForPricingPageSectionsHelper.ChargeCodeGroupList.CustomsDuty,
		};

		string[] GetSupportedChargeGroups()
		{
			switch (Section)
			{
				case ChargeCodeForPricingPageSectionsHelper.Section.Code.OriginPickupCharges:
					return OriginPickupChargesChargeCodeGroupList;
				case ChargeCodeForPricingPageSectionsHelper.Section.Code.DestinationDeliveryCharges:
					return DestinationDeliveryChargesChargeCodeGroupList;
			}

			return Array.Empty<string>();
		}

		public MultilingualString GetInvalidChargeCodeMessage()
		{
			if (Section == ChargeCodeForPricingPageSectionsHelper.Section.Code.OriginPickupCharges)
			{
				return ResString.GetMultilingualString("76dcc3bd-af5a-4866-9cbb-ed4cd48583b0", "Only Charge Codes under Charge Group of OBR,OBO,CDS,ORG,LOD can be selected for Origin Pickup Charges.");
			}
			if (Section == ChargeCodeForPricingPageSectionsHelper.Section.Code.DestinationDeliveryCharges)
			{
				return ResString.GetMultilingualString("e8db22a9-a745-4110-9e56-43dabfffa534", "Only Charge Codes under Charge Group of DST,UNL,BRK,BON,CDS can be selected for Destination Delivery Charges.");
			}

			return ResString.GetMultilingualString("f115da53-1813-4c67-8249-932fa5f240ed", "Enter a valid Charge Code.");
		}

		BusinessObjectCollection fChargeCodeList;

		#endregion

		public void ValidateAllChargeCodesUnderConfiguration()
		{
			foreach(var charge in Charges.Cast<PricingPageChargeCodeGroup>())
			{
				charge.ValidateChargeCodePK();
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			using (SuspendCompositeKeyValidation())
			{
				ValidatePricingPage();
				ValidateSection();
				ValidateAllChargeCodesUnderConfiguration();
			}

			ValidateCompositeKey();
		}

		public IDisposable SuspendCompositeKeyValidation() =>
			new DisposableAction
			(
				() => ++compositeKeyValidationSuspenderIndex,
				() => --compositeKeyValidationSuspenderIndex
			);

		int compositeKeyValidationSuspenderIndex;

		public bool IsCompositeKeyValidationSuspend => compositeKeyValidationSuspenderIndex > 0;

		void ValidateCompositeKey()
		{
			if (IsCompositeKeyValidationSuspend || IsValidationSuspended)
			{
				return;
			}

			var parentCollection = GetParentCollection(this, typeof(ChargeCodeForPricingPageSectionsConfigurationCollection)) as ChargeCodeForPricingPageSectionsConfigurationCollection;
			var configurations = parentCollection?.Cast<ChargeCodeForPricingPageSectionsConfiguration>() ?? Enumerable.Empty<ChargeCodeForPricingPageSectionsConfiguration>();

			var count = configurations
				.Count(x =>
					string.Equals(PricingPage, x.PricingPage, System.StringComparison.OrdinalIgnoreCase) &&
					string.Equals(Section, x.Section, System.StringComparison.OrdinalIgnoreCase));

			if (count > 1)
			{
				AddRowError(IdenticalConfigurationExists);
			}
			else
			{
				RemoveRowError(IdenticalConfigurationExists);
			}
		}

		public static readonly MultilingualString IdenticalConfigurationExists =
			ResString.GetMultilingualString("F585EE38-3E38-4F12-A087-79230F6D6FD2",
				"The same Pricing Page and Section configuration already exists");

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ChargeCodeForPricingPageSectionsConfiguration(fallbackLevel, factory);
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);

			if (Charges != null)
			{
				var chargeCodeForPricingPageSectionsConfigurationClone = (ChargeCodeForPricingPageSectionsConfiguration)clone;
				chargeCodeForPricingPageSectionsConfigurationClone.CloneChargesFrom(chargeCodeForPricingPageSectionsConfigurationClone, Charges);
			}
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.PricingPage, PricingPage);
			writer.WriteElementString(Schema.Section, Section);
			ChargeCodeCollectionSerialiser.Serialize(writer, Charges);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			PricingPage = reader.ReadElementString(Schema.PricingPage);
			Section = reader.ReadElementString(Schema.Section);

			var charges = (PricingPageChargeCodeCollection)ChargeCodeCollectionSerialiser.Deserialize(new XmlRootChangeCompatibilyReader(reader));
			CloneChargesFrom(this, charges);
		}

		class XmlRootChangeCompatibilyReader : XmlReader
		{
			readonly XmlReader inner;

			public XmlRootChangeCompatibilyReader(XmlReader inner)
			{
				this.inner = inner;
			}

			public override int AttributeCount
			{
				get { return inner.AttributeCount; }
			}

			public override string BaseURI
			{
				get { return inner.BaseURI; }
			}

			public override void Close()
			{
				inner.Close();
			}

			public override int Depth
			{
				get { return inner.Depth; }
			}

			public override bool EOF
			{
				get { return inner.EOF; }
			}

			public override string GetAttribute(int i)
			{
				return inner.GetAttribute(i);
			}

			public override string GetAttribute(string name, string namespaceURI)
			{
				return inner.GetAttribute(name, namespaceURI);
			}

			public override string GetAttribute(string name)
			{
				return inner.GetAttribute(name);
			}

			public override bool HasValue
			{
				get { return inner.HasValue; }
			}

			public override bool IsEmptyElement
			{
				get { return inner.IsEmptyElement; }
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "XML Element Name")]
			public override string LocalName
			{
				get { return inner.LocalName == "Charges" ? "ChargeCodes" : inner.LocalName; }
			}

			public override string LookupNamespace(string prefix)
			{
				return inner.LookupNamespace(prefix);
			}

			public override bool MoveToAttribute(string name, string ns)
			{
				return inner.MoveToAttribute(name, ns);
			}

			public override bool MoveToAttribute(string name)
			{
				return inner.MoveToAttribute(name);
			}

			public override bool MoveToElement()
			{
				return inner.MoveToElement();
			}

			public override bool MoveToFirstAttribute()
			{
				return inner.MoveToFirstAttribute();
			}

			public override bool MoveToNextAttribute()
			{
				return inner.MoveToNextAttribute();
			}

			public override XmlNameTable NameTable
			{
				get { return inner.NameTable; }
			}

			public override string NamespaceURI
			{
				get { return inner.NamespaceURI; }
			}

			public override XmlNodeType NodeType
			{
				get { return inner.NodeType; }
			}

			public override string Prefix
			{
				get { return inner.Prefix; }
			}

			public override bool Read()
			{
				return inner.Read();
			}

			public override bool ReadAttributeValue()
			{
				return inner.ReadAttributeValue();
			}

			public override ReadState ReadState
			{
				get { return inner.ReadState; }
			}

			public override void ResolveEntity()
			{
				inner.ResolveEntity();
			}

			public override string Value
			{
				get { return inner.Value; }
			}
		}

		#endregion
	}
}

using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture.Schema;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("UNNumberWithVariant"), WrapperTypeName("UNDGSubstance")]
	public class UNDGSubstanceWrapper : GenericWrapper
	{
		public UNDGSubstanceWrapper(UNDGDataItem dgData, BusinessObjectFactory factory)
			: base(dgData, factory)
		{
			this.DGData = dgData;
		}

		public readonly UNDGDataItem DGData;

		#region Properties

		public ZString StorageCategorySummary
		{
			get
			{
				var undgCountryReferences = DGData?.Subs?.UNDGCountryReferences;

				var storageInstructions = undgCountryReferences?
					.Select(s => ((UNDGCountryReferencePivot)undgCountryReferences.GetRelationshipBusinessObject(s)).DCP_StorageInstruction).ToArray();
				return storageInstructions is null ? ZString.Empty : ZString.Join(", ", storageInstructions);
			}
		}

		public ZBool TankStorageRetentionTrayRequired
		{
			get
			{
				var undgCountryReferences = DGData?.Subs?.UNDGCountryReferences;
				var result = undgCountryReferences?.Any(s => ((UNDGCountryReferencePivot)undgCountryReferences.GetRelationshipBusinessObject(s)).DCP_TankStorageInstructionRetentionTray);
				return result ?? false;
			}
		}

		public PackageWrapper ContainingPackage
		{
			get { return fContainingPackage ?? (fContainingPackage = new PackageWrapperFromFreightPackage(null, Factory)); }
			set { fContainingPackage = value; }
		}
		PackageWrapper fContainingPackage;

		public PackQTYWrapper Packages
		{
			get { return DGData != null ? new PackQTYWrapper(DGData.DI_PackageCount, DGData.DI_F3_NKPackType, DGData.Lookups.PackTypes.GetAsCodeDescriptionPair(), Factory) : PackQTYWrapper.Empty; }
		}

		public ZString SummaryWithPacks
		{
			get { return Packages.Unit.Code.IsEmpty ? Summary : ZString.Format("{0}, {1}", Summary, Packages.ValueAndUnitCode); }
		}

		public ZString UNNumberWithVariant
		{
			get { return DGData != null && DGData.Substance != null ? DGData.Substance.DG_Code : ZString.Empty; }
		}

		public ZString UNNumber
		{
			get { return DGData != null && DGData.Substance != null ? DGData.Substance.DG_UNNO : ZString.Empty; }
		}

		public ZString Variant
		{
			get { return DGData != null && DGData.Substance != null ? DGData.Substance.DG_Variant : ZString.Empty; }
		}

		public ZString Variation
		{
			get { return DGData != null && DGData.Substance != null ? DGData.Substance.DG_Variation : ZString.Empty; }
		}

		public ZString ProperShippingName
		{
			get { return DGData != null && DGData.Substance != null ? DGData.Substance.DG_PSN : ZString.Empty; }
		}

		public ZString LocalName
		{
			get
			{
				var result = ZString.Empty;

				if (DGData != null)
				{
					if (DGData.Substance != null)
					{
						var localLanguage = GlbCompany.CurrentCompany.OrgProxy.OH_Language;
						var localName = DGData.Substance.Names.Find(o => o.DA_Type == ViewUNDGAttributeLookups.TypeConstants.ProperShippingName && o.DA_Language == localLanguage);
						if (localName != null && localName.Any())
						{
							result = localName.First().DA_Descriptor;
						}
						else
						{
							result = DGData.Substance.DG_PSN;
						}
					}
				}

				return result;
			}
		}

		public ViewUNDGAttributeCollection Names
		{
			get
			{
				return psnNames ?? (DGData != null && DGData.Substance != null ? (psnNames = GetNames()) : null);
			}
		}
		ViewUNDGAttributeCollection psnNames;

		ViewUNDGAttributeCollection GetNames()
		{
			var collection = new ViewUNDGAttributeCollection(DGData.Substance, ViewUNDGAttributeLookups.TypeConstants.ProperShippingName);
			var names = DGData.Substance.Names.Where(o => o.DA_Type == ViewUNDGAttributeLookups.TypeConstants.ProperShippingName);
			if (names != null)
			{
				collection.AddRange(names);
			}

			return collection;
		}

		public ZString IMOClass
		{
			get
			{
				var result = ZString.Empty;

				if (DGData != null)
				{
					if (!DGData.DI_IMOClass.IsEmpty)
					{
						result = DGData.DI_IMOClass;
					}
					else if (DGData.Substance != null)
					{
						result = DGData.Substance.DG_Class;
					}
				}

				return result;
			}
		}

		public ZString SubLabel1
		{
			get { return DGData != null && DGData.Substance != null ? DGData.Substance.DG_SubLabel1 : ZString.Empty; }
		}

		public ZString SubLabel2
		{
			get { return DGData != null && DGData.Substance != null ? DGData.Substance.DG_SubLabel2 : ZString.Empty; }
		}

		public ZString PackingGroup
		{
			get { return DGData != null && DGData.Substance != null ? DGData.Substance.DG_PG : ZString.Empty; }
		}

		public ZString PSAGroup
		{
			get { return DGData != null && DGData.IsPSAGroupApplicable ? DGData.PSAGroup : ZString.Empty; }
		}

		public ZString PSAGroupWithLabel
		{
			get { return !PSAGroup.IsEmpty ? Res.GetString("ab77e6c3-5946-4a7e-815f-238b38ba28e9", "PSA Group: {0}", PSAGroup) : string.Empty; }
		}

		public ZString PackingInstructions
		{
			get { return DGData != null && DGData.Substance != null ? DGData.Substance.DG_PackIns : ZString.Empty; }
		}

		public ZBool IsLimitedQuantity
		{
			get { return DGData != null && DGData.DI_IsLimitedQuantity; }
		}

		public ZString EMSCode
		{
			get { return DGData != null && DGData.Substance != null ? DGData.Substance.DG_EMS : ZString.Empty; }
		}

		public ZString FlashPoint
		{
			get
			{
				ZString result = ZString.Empty;

				if (DGData != null)
				{
					if (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.Value)
					{
						if (DGData.DI_IsCombustible)
						{
							result = DGData.DI_DGFlashPoint.ToString();
						}
					}
					else
					{
						if (!DGData.DI_DGFlashPoint.IsEmpty)
						{
							result = DGData.DI_DGFlashPoint.ToString();
						}
						else if (DGData.Substance != null)
						{
							result = DGData.Substance.DG_FlashPoint;
						}
					}
				}

				return result;
			}
		}

		public ZString MarinePollutantWarning
		{
			get
			{
				ZString mP = ZString.Empty;

				if (DGData != null)
				{
					if (!DGData.DI_MPMarinePollutant.IsEmpty)
					{
						mP = DGData.DI_MPMarinePollutant;
					}
					else if (DGData.Substance != null && DGData.Substance.DG_MP != UNDGSubstanceLookups.MarinePollutantTypes.Depends_Code)
					{
						mP = DGData.Substance.DG_MP;
					}
				}

				if (mP.IsEmpty)
				{
					return ZString.Empty;
				}

				var routingSupporter = ContainingPackage?.Parent?.WrappedObject as IRoutingSupport;
				var appendEnvironmentallyHazardousWarning = routingSupporter != null
					&& routingSupporter.TransportsIncludingRelated.Cast<Transport>().Any(t => ADNStandardSummaryWriter.TransportLegIsBargeLegInADNContractingCountry(t))
					&& routingSupporter.TransportsIncludingRelated
						.Where(t => t.JW_TransportMode == Constants.TransportModes.Sea || t.JW_TransportMode == Constants.TransportModes.InlandWaterwayTransport)
						.All(t => t.Vessel != null && t.Vessel.RV_VesselType == Constants.VesselType.Barge);

				return appendEnvironmentallyHazardousWarning
					? (ZString)Res.GetString("531d3bc6-22c4-4b4c-af06-b5e2f484235c", "MARINE POLLUTANT / ENVIRONMENTALLY HAZARDOUS")
					: (ZString)Res.GetString("b8b8b1c0-8c5f-4636-a011-8a1cfa528757", "MARINE POLLUTANT");
			}
		}

		public ZString TechnicalName
		{
			get
			{
				if (fTechnicalName.IsEmpty)
				{
					fTechnicalName = DGData == null ? ZString.Empty : DGData.DI_TechnicalName;
				}
				return fTechnicalName;
			}
			set { fTechnicalName = value; }
		}

		ZString fTechnicalName;

		#region Weight

		public WeightWrapper Weight
		{
			get { return weight ?? (weight = GetWeight()); }
		}

		WeightWrapper weight;

		protected virtual WeightWrapper GetWeight()
		{
			return DGData != null ? new WeightWrapper(DGData.DI_DGWeight, DGData.DI_UnitOfWeight, UNDGDataItemSchema.DI_DGWeight.Scale, DGData.Lookups.WeightUnits, Factory) : WeightWrapper.Empty;
		}

		#endregion

		#region Volume

		public VolumeWrapper Volume
		{
			get { return volume ?? (volume = GetVolume()); }
		}

		VolumeWrapper volume;

		protected virtual VolumeWrapper GetVolume()
		{
			return DGData != null ? new VolumeWrapper(DGData.DI_DGVolume, DGData.DI_UnitOfVolume, DGData.Lookups.VolumeUnits, Factory) : VolumeWrapper.Empty;
		}

		#endregion

		#region Summary

		// this hint assumes all IUNDGStandardWriters have the component PackingGroupComponent
		[CodeStringFinderHint(typeof(PackingGroupComponent), "Enterprise.DocumentWrappers.IUNDGSummaryWriterComponent.Write")]
		public ZString Summary
		{
			get
			{
				var standardSummaryWriter = StandardSummaryWriterFactory.GetWriter(DGData?.Subs?.DG_Standard);
				bool standardConditionApplies = standardSummaryWriter.StandardConditionApplies(this);

				var summaries = standardSummaryWriter
					.Components
					.Where(component => (standardConditionApplies || component.IsDefault) && component is not EMSComponent)
					.Select(component => component.Write(this))
					.Where(summary => !summary.IsEmpty)
					.ToArray();

				return ZString.Join(", ", summaries);
			}
		}

		#endregion

		#region SummaryWithEMSCode

		// this hint assumes all IUNDGStandardWriters have the component PackingGroupComponent
		[CodeStringFinderHint(typeof(PackingGroupComponent), "Enterprise.DocumentWrappers.IUNDGSummaryWriterComponent.Write")]
		public ZString SummaryWithEMSCode
		{
			get
			{
				var standardSummaryWriter = StandardSummaryWriterFactory.GetWriter(DGData?.Subs?.DG_Standard);
				bool standardConditionApplies = standardSummaryWriter.StandardConditionApplies(this);

				var summaries = standardSummaryWriter
					.Components
					.Where(component => standardConditionApplies || component.IsDefault)
					.Select(component => component.Write(this))
					.Where(summary => !summary.IsEmpty)
					.ToArray();

				return ZString.Join(", ", summaries);
			}
		}

		#endregion

		#region SummaryWithContainingPackageID

		public ZString SummaryWithContainingPackageID
		{
			get
			{
				var result = new ZStringBuilder(Summary);
				if (!ContainingPackage.Packages.Unit.Code.IsEmpty && ContainingPackage.Packages.Value > 0)
				{
					result.Append(Res.GetString("29b73b96-e50a-41ce-96a5-250642301f0b", ", in {0}x {1}", ContainingPackage.Packages.Value, ContainingPackage.Packages.Unit.Code));
					var packageID = ContainingPackage.RefNumber;
					if (!packageID.IsEmpty)
					{
						result.Append(" " + packageID);
					}

					var containerID = ContainingPackage.ContainerNo;
					if (!containerID.IsEmpty && packageID != containerID)
					{
						result.Append(Res.GetString("26B50202-41BB-4A06-838E-F2CCB62A09F3", ", in container {0}", containerID));
					}
				}
				return result.ToString();
			}
		}

		#endregion

		#region LtdQty

		public ZString LtdQty
		{
			get
			{
				if (IsLimitedQuantity)
				{
					if ((DGData?.Substance?.DG_Standard ?? ZString.Empty) == UNDGSubstanceLookups.UNDGSubstanceStandardTypes.ADR)
					{
						return " " + "LQ";
					}

					return " " + Res.GetString("a348bcef-4db0-4caa-9225-f4022c49f07d", "LTD QTY");
				}

				return ZString.Empty;
			}
		}

		#endregion

		#region DGContact

		public ContactWrapper DGContact
		{
			get { return DGData == null ? new ContactWrapper(Factory.GetNull<OrgContact>(), Factory) : new ContactWrapper(DGData.DGContact, Factory); }
		}

		#endregion

		#endregion
	}
}

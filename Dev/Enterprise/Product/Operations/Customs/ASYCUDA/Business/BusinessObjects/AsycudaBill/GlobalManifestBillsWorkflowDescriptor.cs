using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Universal.Helper;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business
{
#if DEBUG
	[CargoWise.Common.Testing.SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
#endif
	public class GlobalManifestBillsWorkflowDescriptor : WorkflowDescriptor
	{
		#region ID / Description

		public override string Code
		{
			get { return WorkflowDescriptors.GlobalManifestBillsWorkflowDescriptorCode; }
		}

		public override IMultilingualString Description
		{
			get { return ResString.GetMultilingualString("GlobalManifestBillsWorkflowDescriptor|Description", "Global Manifest Bills"); }
		}

		public override ZString MilestoneTemplateHintCaption
		{
			get { return ""; }
		}

		#endregion

		#region Sub Types

		public override ProcessTemplateSubType[] SubTypeInformation
		{
			get
			{
				List<ProcessTemplateSubType> list = new List<ProcessTemplateSubType>();
				list.Add(new ProcessTemplateSubType(Res.GetString("586BB79B-F0E1-4E69-90EC-D30582D1998A", "Manifest Country"), Countries));
				list.Add(new ProcessTemplateSubType(Res.GetString("82A8A596-11FE-4075-AD80-5DBAC839B4B7", "Manifest Type"), ManifestTypesList));
				list.Add(new ProcessTemplateSubType(Res.GetString("5C36125B-E3FC-4081-8C9F-46A0F2A0D58A", "Bill Shipment Type"), ShipmentTypesList));
				return list.ToArray();
			}
		}

		#endregion

		public CodeDescriptionPairList Countries
		{
			get
			{
				var factory = LastProcessTaskTemplate != null ? LastProcessTaskTemplate.Factory : CreateNewFactory();

				return factory.GetCachedValue("GMB.All.Supported.Countries", delegate
				{
					var baseList = new RefCountryCollection(factory);
					var f = new ZQuery();
					f.AddToFilter(RefCountrySchema.RN_Code, CountryHelper.SupportedCountries(factory));
					baseList.AdditionalFilter = f;
					baseList.ApplySort(RefCountrySchema.RN_Desc.Name, System.ComponentModel.ListSortDirection.Ascending);

					var countriesList = new CodeDescriptionPairList();
					countriesList.AddRange(baseList);
					return countriesList;
				});
			}
		}

		CodeDescriptionPairList ManifestTypesList
		{
			get
			{
				if (LastProcessTaskTemplate != null)
				{
					var subType1 = LastProcessTaskTemplate.P0_SubType1;
					if (!subType1.IsEmpty)
					{
						return AsycudaManifestHeaderLookups.GetAllApplicationManifestTypes(LastProcessTaskTemplate.Factory);
					}
				}

				return new CodeDescriptionPairList();
			}
		}

		public CodeDescriptionPairList ShipmentTypesList
		{
			get { return LastProcessTaskTemplate != null ? LastProcessTaskTemplate.Factory.GetCachedValue<ShipmentTypeList>() : new ShipmentTypeList(); }
		}

		#region Criteria Requirements

		public override bool RequiresClient { get { return false; } }
		public override bool RequiresBranch { get { return true; } }
		public override bool RequiresDepartment { get { return false; } }
		public override bool RequiresPort1 { get { return false; } }
		public override bool RequiresPort2 { get { return false; } }
		public override bool SupportsEventTracking { get { return false; } }
		public override bool SupportsTasks { get { return false; } }

		public override bool SupportsScreenLayout { get { return false; } }

		#endregion

		public override Type WorkflowProviderType => typeof(AsycudaBill);

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.Customs.ASYCUDA.ASYCUDAManifestBill; }
		}

		protected override FormCustomisationSettingsProvider GetFormCustomisationSettingsProvider()
		{
			return new GlobalManifestBillsFormCustomisationSettingsProvider();
		}

		public override bool SupportsUniversalTemplates => false;

		public override bool SupportsBufferManagement => false;
	}
}

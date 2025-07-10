using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.Common.PostUpdateProcesses.ScreeningStatus
{
	class RefVesselScreeningStatusPostUpdateProcess : ScreeningStatusPostUpdate<RefVessel>
	{
		public RefVesselScreeningStatusPostUpdateProcess(IEntityContext context, IEntity rootEntity) : base(context, rootEntity)
		{
		}

		protected override bool ShouldRun
		{
			get
			{
				var statistics = Context.Statistics;
				return RootEntity != null &&
					   RootEntity.TableName == RefVesselSchema.Constants.TableName &&
					   RootEntity.Action != EntityAction.DELETE &&
					   statistics.GetEntityStatistics(RefVesselSchema.Constants.TableName).Entities.Count > 0;
			}
		}

		protected override void SetScreeningStatus(ZGuid pk, string screeningStatus)
		{
			var vesselRow = Context.RowFactory.GetRow(RefVesselSchema.Constants.TableName, pk);
			if (vesselRow != null)
			{
				vesselRow[RefVesselSchema.Constants.RV_ScreeningStatus] = screeningStatus;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		protected override bool HasChanges(RefVessel vessel)
		{
			return vessel != null &&
				   !vessel.IsDeleted &&
				   (PropertyValueComparisonUtils.StringValueHasChanges(RootEntity, "RadioCallSign", vessel.RV_RadioCallSign.ToString()) ||
					PropertyValueComparisonUtils.StringValueHasChanges(RootEntity, "LloydsNumber", vessel.RV_LloydsNumber.ToString()) ||
					PropertyValueComparisonUtils.BoolValueHasChanges(RootEntity, "IsActive", vessel.RV_IsActive) ||
					PropertyValueComparisonUtils.StringValueHasChanges(RootEntity, "Code", vessel.RV_Code.ToString()) ||
					ShippingProviderChangedAndScreenStatusNotSame(vessel) ||
					CountryOfRegInfoHasChanges(vessel));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		bool CountryOfRegInfoHasChanges(RefVessel vessel)
		{
			var result = false;
			var countryOfRegEntity = RootEntity.Parents.FirstOrDefault(x => x.EntityName == "CountryOfReg");
			if (countryOfRegEntity != null)
			{
				result = PropertyValueComparisonUtils.StringValueHasChanges(countryOfRegEntity, "Code", vessel.RV_RN_NKCountryOfReg.ToString());
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		bool ShippingProviderChangedAndScreenStatusNotSame(RefVessel vessel)
		{
			var result = false;
			var shippingProvider = RootEntity.Parents.FirstOrDefault(x => x.EntityName == "OrgHeader");
			if (shippingProvider != null)
			{
				var pk = shippingProvider.InternalPK;
				var code = shippingProvider.GetPropertyOrBlankString("Code");

				OrgHeader header = null;

				if (pk != Guid.Empty)
				{
					header = Context.ObjectFactory.Load<OrgHeader>(pk);
				}

				if (header == null && !string.IsNullOrEmpty(code))
				{
					header = Context.ObjectFactory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, code);
				}

				if (header != null && header.PK != vessel.RV_OH)
				{
					result = header.OH_ScreeningStatus != vessel.RV_ScreeningStatus;
				}
			}

			return result;
		}
	}
}

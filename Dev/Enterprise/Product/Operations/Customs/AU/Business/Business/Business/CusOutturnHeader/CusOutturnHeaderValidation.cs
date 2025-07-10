using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusOutturnHeaderValidation : Customs.Business.CusOutturnHeaderValidation
	{
		public CusOutturnHeaderValidation(CusOutturnHeader parent)
			: base(parent)
		{
			this.parent = parent;
		}
		readonly CusOutturnHeader parent;

		protected override void CheckC6_VesselName()
		{
			base.CheckC6_VesselName();
			var header = Parent;
			if (!header.C6_VesselName.IsEmpty && !header.C6_LloydsIMO.IsEmpty)
			{
				CheckVesselExist(header.C6_VesselNameInfo);
			}
		}

		protected override void CheckC6_LloydsIMO()
		{
			base.CheckC6_LloydsIMO();
			var header = Parent;
			MandatoryValidation.CheckEntered(header.C6_LloydsIMOInfo);
			CheckUnique(header.C6_LloydsIMOInfo);

			ValidateC6_VesselName();
		}

		protected override void CheckC6_OutturningPremiseID()
		{
			base.CheckC6_OutturningPremiseID();
			MandatoryValidation.CheckEntered(Parent.C6_OutturningPremiseIDInfo);
			CheckUnique(Parent.C6_OutturningPremiseIDInfo);
			EDIMessage orgMessage = parent.Messages.GetLastMessage(EDIMessage.ApplicationCodes.CMR, CMRMessage.CMRMessageTypes.SEAOUT,
					EDIMessage.Direction.Transmit, ZString.Empty, CMRMessage.MessageSubTypes.Original);
			if (orgMessage != null && orgMessage.Interchange != null && !orgMessage.Interchange.EI_From.IsEmpty &&
					GlbCompany.CurrentCompany.GC_CustomsRegistrationNo != orgMessage.Interchange.EI_From)
			{
				parent.C6_OutturningPremiseIDInfo.AddMessageError("This outturn was originally created under a different Customs Site Id to that specified in the current company's Customs Reg No field.");
			}
		}

		protected override void CheckC6_VoyageNum()
		{
			base.CheckC6_VoyageNum();
			MandatoryValidation.CheckEntered(Parent.C6_VoyageNumInfo);
			CheckUnique(Parent.C6_VoyageNumInfo);
		}

		#region Uniqueness

		void CheckUnique(ZPropertyInfo property)
		{
			if (!IsUnique)
			{
				property.AddError("An Outturn Header already exists with the same Vessel, Voyage and Premise ID details. Please go to your existing Outturn Header and enter your lines there.");
			}
		}

		bool IsUnique
		{
			get { return Parent.Factory.LoadTop1<CusOutturnHeader>(UniqueFieldsFilter) == null; }
		}

		ZQuery UniqueFieldsFilter
		{
			get
			{
				if (uniqueFieldsFilter == null)
				{
					uniqueFieldsFilter = new ZQuery();
					uniqueFieldsFilter.AddToFilter(CusOutturnHeaderSchema.C6_VoyageNum, Parent.C6_VoyageNum);
					uniqueFieldsFilter.AddToFilter(CusOutturnHeaderSchema.C6_OutturningPremiseID, Parent.C6_OutturningPremiseID);
					uniqueFieldsFilter.AddToFilter(CusOutturnHeaderSchema.C6_LloydsIMO, Parent.C6_LloydsIMO);
					uniqueFieldsFilter.AddToFilter(CusOutturnHeaderSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
				}
				return uniqueFieldsFilter;
			}
		}
		ZQuery uniqueFieldsFilter;

		#endregion

		#region VesselExist
		void CheckVesselExist(ZPropertyInfo property)
		{
			if (!IsExist)
			{
				property.AddWarning("Vessel is not on file.");
			}
		}

		bool IsExist
		{
			get { return Parent.Factory.ExistsInDatabase(RefVesselSchema.Constants.TableName, ExistFilter); }
		}

		ZQuery ExistFilter
		{
			get
			{
				if (existFilter == null)
				{
					existFilter = new ZQuery(RefVesselSchema.RV_Code, Parent.C6_VesselName);
					existFilter.AddToFilter(RefVesselSchema.RV_LloydsNumber, Parent.C6_LloydsIMO);
				}
				return existFilter;
			}
		}
		ZQuery existFilter;
		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateChildUnique();
		}

		#endregion

		#region Child Uniqueness

		public void ValidateChildUnique()
		{
			var outturns = parent.Outturns;
			if (outturns.Count > 0)
			{
				var lastSystemCreateTimeUtc = ZDateTime.Empty;
				var outturnsIncludingReloaded = new HashSet<DepotCusOutturn>();
				foreach (DepotCusOutturn outturn in outturns)
				{
					outturnsIncludingReloaded.Add(outturn);
					if (outturn.IsInDatabase)
					{
						var newSystemCreateTimeUtc = outturn.C5_SystemCreateTimeUtc;
						if (newSystemCreateTimeUtc > lastSystemCreateTimeUtc || !lastSystemCreateTimeUtc.IsValid)
						{
							lastSystemCreateTimeUtc = newSystemCreateTimeUtc;
						}
					}
				}
				var queryForNewOutturns = new ZDBOnlyQuery(typeof(DepotCusOutturn));
				queryForNewOutturns.IgnoreDbQueryCache = true;
				queryForNewOutturns.AddToFilter(CusOutturnSchema.C5_C6, parent.PK);
				if (!lastSystemCreateTimeUtc.IsEmpty)
				{
					queryForNewOutturns.AddToFilter(CusOutturnSchema.C5_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, lastSystemCreateTimeUtc);
				}
				var addedOutturnPKs = new HashSet<ZGuid>();
				foreach (var newOutturn in parent.Factory.Load<DepotCusOutturn>(queryForNewOutturns))
				{
					if (!outturns.Contains(newOutturn.PK))
					{
						outturns.Add(newOutturn);
						outturnsIncludingReloaded.Add(newOutturn);
						addedOutturnPKs.Add(newOutturn.PK);
					}
				}
				var uniqueValidator = new DepotCusOutturnsUniqueValidator(outturnsIncludingReloaded);
				uniqueValidator.BuildRepeatedElements();

				foreach (var outturn in outturnsIncludingReloaded)
				{
					outturn.RemoveRowError(messageError1);
					outturn.RemoveRowError(messageError2);
					outturn.RemoveRowError(messageError3);

					if (uniqueValidator.HasError(outturn.PK))
					{
						if (addedOutturnPKs.Contains(outturn.PK))
						{
							outturn.AddRowError(messageError3);
						}
						else
						{
							outturn.AddRowError(outturn.C5_HouseBill.IsEmpty && outturn.C5_MasterBill.IsEmpty ? messageError2 : messageError1);
						}
					}
				}
			}
		}

		internal const string messageError1 = "An Outturn line already exists with the same Cargo Type, Container Number, House Bill and Ocean Bill details.";
		internal const string messageError2 = "An Outturn line already exists with the same Container Number and no House Bill or Ocean Bill details.";
		internal const string messageError3 = "This duplicate row was added by others. Please confirm and remove any duplicates.";

		#endregion
	}
}

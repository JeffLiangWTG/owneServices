using CargoWise.EntityFramework;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManTranHeadValidation : Customs.Business.CusSeaManTranHeadValidation
	{
		public CusSeaManTranHeadValidation(CusSeaManTranHead parent)
			: base(parent)
		{
			this.head = parent;
		}

		protected override void CheckBT_VoyageNum()
		{
			base.CheckBT_VoyageNum();

			MandatoryValidation.CheckEntered(head.BT_VoyageNumInfo);

			if (head.BT_VoyageNum.Length > 6)
			{
				head.BT_VoyageNumInfo.AddMessageError("Voyage Number cannot be longer than 6 characters.");
			}
		}

		protected override void CheckBT_VesselName()
		{
			base.CheckBT_VesselName();

			var info = head.BT_VesselNameInfo;

			MandatoryValidation.CheckEntered(info);

			head.BT_VesselNameInfo.ValidateVesselIsValid(() => head.Vessel);
			if (head.Vessel == null)   // This will / (can) occur once removing the unique constraint on Vessel RV_Code(Name) is implemented and duplicate vessel names can be created in the reference table.
			{
				if (head.VesselHasDuplicates)
				{
					head.BT_VesselNameInfo.AddMessageError(Res.GetString("FFBE1C57-6DF9-41C0-BFB3-77596BF95C4A", "Duplicate Vessels exist for this Vessel Name.\r\nUse the <F4> key to show all vessels with this name for appropriate selection of the required vessel."));
				}
			}
			else
			{
				var lloydsValidation = new LloydsNumberValidation();
				lloydsValidation.Validate(head.Vessel.RV_LloydsNumber);
				if (!lloydsValidation.IsValid)
				{
					head.BT_VesselNameInfo.AddMessageError(lloydsValidation.ErrorText);
				}
			}
		}

		protected override void CheckBT_RL_NKPortOfLastForeignPort()
		{
			base.CheckBT_RL_NKPortOfLastForeignPort();

			var info = head.BT_RL_NKPortOfLastForeignPortInfo;

			MessageValidation.CheckEntered(info);

			if (!info.Value.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(info, head.Lookups.PortOfLastForeignPorts);

				var lastForeignPort = head.PortOfLastForeignPort;
				if (lastForeignPort != null && lastForeignPort.TimeZoneSet == null)
				{
					info.AddMessageError($"Please choose a valid time zone on {info.HumanReadableName}");
				}
			}
		}

		protected override void CheckBT_PortOfLastForeignPortATD()
		{
			base.CheckBT_PortOfLastForeignPortATD();

			MessageValidation.CheckEntered(head.BT_PortOfLastForeignPortATDInfo);
		}

		#region Implementation

		MessageValidation MessageValidation
		{
			get
			{
				if (fMessageValidation == null)
				{
					fMessageValidation = new MessageValidation(head);
				}
				return fMessageValidation;
			}
		}
		MessageValidation fMessageValidation;

		readonly CusSeaManTranHead head;

		#endregion
	}
}

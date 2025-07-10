using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentWrappers
{
	static class ServiceInstructionHelper
	{
		// tested in FreightWrapperFromDtbBooking
		public static ZString GetHandlingInstructionsWithResultAppended(DtbBooking transportBooking, string result = "")
		{
			var builder = new ZStringBuilder(result);
			AppendNote(transportBooking, PredefinedNoteTypes.Instance.HandlingInstructions, builder);
			AppendNote(transportBooking, PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation, builder);
			if (transportBooking.ParentJob != null)
			{
				AppendNote(transportBooking.ParentJob.ParentWithWorkflow, PredefinedNoteTypes.Instance.HandlingInstructions, builder);
				AppendNote(transportBooking.ParentJob.ParentWithWorkflow, PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation, builder);
			}

			return GetHandlingInstructionsAppended(transportBooking.Address.Organisation, builder);
		}

		public static ZString GetHandlingInstructionsWithResultAppended(OrgHeader org, ZString result)
		{
			return GetHandlingInstructionsAppended(org, new ZStringBuilder(result));
		}

		static ZString GetHandlingInstructionsAppended(OrgHeader org, ZStringBuilder builder)
		{
			AppendNote(org, PredefinedNoteTypes.Instance.HandlingInstructions, builder);
			AppendNote(org, PredefinedNoteTypes.Instance.SpecialInstructions, builder);

			return builder.ToStringWithNewLineBetweenAppends().Trim();
		}

		static void AppendNote(BusinessObject bizO, PredefinedNoteType noteTypeOnBizO, ZStringBuilder noteToAppendTo)
		{
			if (bizO != null)
			{
				foreach (var note in bizO.GetNotes().FindByDescription(noteTypeOnBizO.Description))
				{
					if (!noteToAppendTo.ToString().Contains(note.ST_NoteText))
					{
						noteToAppendTo.Append(note.ST_NoteText);
					}
				}
			}
		}
	}
}

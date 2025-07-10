using System.Collections;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public static class FreightHelperClass
	{
		public static ZString FormatETAETDDate(ZBool transportModeIsSea, ZDateTime date)
		{
			if (!date.IsEmpty)
			{
				if (transportModeIsSea)
				{
					return date.ToShortDateString();
				}
				else
				{
					return date.ToLongTimeString();
				}
			}
			else
			{
				return ZString.Empty;
			}
		}

		public static ZString FormatBillAndIssueDate(ZString billNumber, ZDateTime issueDate)
		{
			if (issueDate.IsEmpty)
			{
				return billNumber;
			}
			else
			{
				return billNumber + " / " + issueDate.ToShortDateString();
			}
		}

		public static ZString FormatBillAndIssueHeading(ZString billHeading, ZDateTime issueDate)
		{
			if (issueDate.IsEmpty)
			{
				return billHeading;
			}
			else
			{
				return Res.GetString("4f2961cc-d32f-4b25-afe8-46a3087777d5", "{0} / ISSUE", billHeading);
			}
		}

		public static ZString MergePackMarksAndNumbers(ICollection packLines)
		{
			List<ZString> list = new List<ZString>();

			foreach (PackLine packLine in packLines)
			{
				ZString marks = packLine.JL_MarksAndNumbers.Trim();

				if (!list.Contains(marks))
				{
					list.Add(marks);
				}
			}

			return ZString.Join(System.Environment.NewLine, list.ToArray());
		}

		public static int GetNumberOfDecimalsForTransportModeAndUnit(ZString transportMode, ZString unitOfMeasure)
		{
			int registrySetNumberOfDecimals = FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.Value.GetNumberOfDecimals(transportMode, unitOfMeasure);
			if (registrySetNumberOfDecimals != -1)
			{
				return registrySetNumberOfDecimals;
			}

			if (Core.Constants.Weight.ContainsCode(unitOfMeasure))
			{
				return WeightWrapper.StandardDecimalPlaces;
			}
			else if (Core.Constants.Volume.ContainsCode(unitOfMeasure))
			{
				return VolumeWrapper.StandardDecimalPlaces;
			}

			return 3;
		}

		#region Notes

		public static void AddNote(BusinessObject bizo, ZString noteDesc, ZString noteData)
		{
			AddNote(bizo, noteDesc, noteData, StmNoteContextUtils.StmNoteContextsAll);
		}

		public static void AddNote(BusinessObject bizo, ZString noteDesc, ZString noteData, StmNoteContexts context)
		{
			var note = bizo.GetNotes().AddNew();
			note.ST_Description = noteDesc;
			note.ST_ParentID = bizo.PK;
			note.ST_Table = bizo.TableName;
			note.ST_NoteDataAsText = noteData;
			note.ST_NoteContext = context.ToString();
		}
		#endregion
	}
}

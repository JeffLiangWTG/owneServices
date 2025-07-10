using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IssueManager.Business
{
	public class LogCloser
	{
		public LogCloser(BusinessObject[] selectedElements)
		{
			this.selectedElements = selectedElements;
			savingFactory = new BusinessObjectFactory();
		}

		public string Errors
		{
			get { return errors; }
		}

		public bool HasDBChanged
		{
			get { return hasDBChanged; }
		}

		public void Close()
		{
			errors = "";
			hasDBChanged = false;

			ZQuery filter = new ZQuery(HelpErrorLogSchema.PK, Array.ConvertAll(selectedElements, item => item.PK));

			EdiHelpErrorLog[] logsToClose = savingFactory.Load<EdiHelpErrorLog>(filter);

			if (logsToClose.Length == selectedElements.Length)
			{
				foreach (EdiHelpErrorLog errorLog in logsToClose)
				{
					if (errorLog.HE_FixedDate.IsEmpty)
					{
						ZQuery latestExeDate = new ZQuery(HelpErrorLogOccurrenceSchema.HO_HE, errorLog.PK);
						latestExeDate.OrderBy = HelpErrorLogOccurrenceSchema.Constants.HO_EXEDateTime + " DESC";
						HelpErrorLogOccurrence latestExeOccurrence = savingFactory.LoadTop1<HelpErrorLogOccurrence>(latestExeDate);
						if (latestExeOccurrence != null)
						{
							errorLog.HE_FixedDate = latestExeOccurrence.HO_EXEDateTime;
						}
						else
						{
							errorLog.HE_FixedDate = ZDateTime.UtcNow;
						}
					}

					errorLog.RunPreSaveValidation();

					if (errorLog.HasErrors)
					{
						errors += string.Join(System.Environment.NewLine, errorLog.Notifications.GetErrors().Select(x => x.Message));
					}
				}

				if (errors.Length == 0)
				{
					savingFactory.Save();
				}
			}
			else
			{
				hasDBChanged = true;
			}
		}

		readonly BusinessObjectFactory savingFactory;
		readonly BusinessObject[] selectedElements;

		string errors;
		bool hasDBChanged;
	}
}


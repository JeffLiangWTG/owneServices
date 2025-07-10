using System;
using System.IO;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DataConverters
{
	public abstract class DataWriter
	{
		public DataWriter(BusinessObjectFactory factory)
		{
			this.Factory = factory;
		}
		protected readonly BusinessObjectFactory Factory;

		public void SaveRecordToEnterprise(bool excludeExistingRecords, ProgressLogger log)
		{
			if (log == null)
			{
				throw new ArgumentNullException("Log");
			}

			ZString anyReasonRecordShouldBeExcluded = GetAnyReasonRecordShouldBeExcluded();
			if (anyReasonRecordShouldBeExcluded.IsEmpty)
			{
				BusinessObject existingBusinessObject = GetExistingBusinessObject();
				try
				{
					if (existingBusinessObject == null)
					{
						BusinessObject newBusinessObject = GetNewBusinessObject();
						TurnOffValidationAndUpdateBusinessObject(newBusinessObject, log);
						newBusinessObject.GetLogs().AddNew(Events.DataImport);
						log.RecordsCreated++;
					}
					else
					{
						if (excludeExistingRecords)
						{
							log.Add("Excluded " + RecordDescription + " - already exists in a " + BrandingFactory.Instance.ProductName + " database table");
							Factory.ClearQueryCache();
							log.RecordsExcluded++;
						}
						else
						{
							TurnOffValidationAndUpdateBusinessObject(existingBusinessObject, log);
							existingBusinessObject.GetLogs().AddNew(Events.DataImport);
							log.RecordsUpdated++;
						}
					}
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					log.DisplayFormatLogMessage(RecordDescription, e.Message);
					log.RecordsInvalid++;
				}
			}
			else
			{
				log.Add("Excluded " + RecordDescription + " - " + anyReasonRecordShouldBeExcluded);
				Factory.ClearQueryCache();
				log.RecordsExcluded++;
			}
		}

		protected void TurnOffValidationAndUpdateBusinessObject(BusinessObject businessObjectToUpdate, ProgressLogger log)
		{
			businessObjectToUpdate.SuspendValidation();
			UpdateEnterpriseValues(businessObjectToUpdate);
		}

		public void AddRecordToCSVFile(ZString templateFile, ProgressLogger log)
		{
			ZString anyReasonRecordShouldBeExcluded = GetAnyReasonRecordShouldBeExcluded();
			if (anyReasonRecordShouldBeExcluded.IsEmpty)
			{
				using (StreamWriter sw = File.AppendText(templateFile))
				{
					sw.WriteLine(CSVOutputLine);
				}
				log.RecordsCreated++;
			}
			else
			{
				log.Add("Excluded " + RecordDescription + " - " + anyReasonRecordShouldBeExcluded);
				log.RecordsExcluded++;
			}
		}

		protected internal ZString RemoveComma(ZString text)
		{
			return text.Replace(",", "");
		}

		public abstract ZString RecordDescription { get; }
		protected abstract internal ZString GetAnyReasonRecordShouldBeExcluded();
		protected abstract BusinessObject GetNewBusinessObject();
		protected abstract BusinessObject GetExistingBusinessObject();
		protected abstract void UpdateEnterpriseValues(BusinessObject businessObjectToUpdate);

		public virtual BusinessObject CurrentBusinessObject
		{
			get { return null; }
		}

		protected virtual internal ZString CSVOutputLine
		{
			get { return ZString.Empty; }
		}
	}
}

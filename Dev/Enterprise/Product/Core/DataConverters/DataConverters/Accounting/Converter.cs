using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DataConverters.Accounting
{
	public struct ConverterData
	{
		public readonly int CurrentRow;
		public readonly StringCollection Errors;
		public readonly ZDecimal TotalAmountImported;

		public ConverterData(int currentRow, StringCollection errors, ZDecimal totalAmountImported)
		{
			this.CurrentRow = currentRow;
			this.Errors = errors;
			this.TotalAmountImported = totalAmountImported;
		}
	}

	public class Converter : AccountingConverterBase
	{
		public Converter()
		{
			BusinessObjectFactoryList = new ArrayList();
			Errors = new StringCollection();
		}

		public event EventHandler ProgressChanged;
		public event EventHandler ErrorOccurred;
		public event EventHandler ImportCompleted;

		public void Stop()
		{
			IsStop = true;
		}

		public void SaveChanges()
		{
			BusinessObjectFactory.SaveTogether(JournalFactories);
		}

		public void ClearErrors()
		{
			if (Errors != null && Errors.Count != 0)
			{
				Errors.Clear();
			}
		}

		public virtual Journal[] ImportFile(string fileName, bool isDebtors, ZDateTime postDate)
		{
			this.FileName = fileName;
			this.PostDate = (postDate == ZDateTime.Empty ? ZDateTime.Now : postDate);
			Ledger = isDebtors ? LedgerTypes.AccountsReceivable : LedgerTypes.AccountsPayable;
			ClearingAccount = Ledger == LedgerTypes.AccountsReceivable ? (Guid)AccountingConfigurationRegistry.Instance.ARJournalAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty) : (Guid)AccountingConfigurationRegistry.Instance.APJournalAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);

			Journal[] createdJournals = null;

			if (ValidateFile() && ValidateClearingAccount())
			{
				createdJournals = ProcessFile(fileName, postDate);
			}

			RaiseImportCompletedEvent();
			return createdJournals;
		}

		protected void RaiseImportCompletedEvent()
		{
			if (ImportCompleted != null)
			{
				ImportCompleted(null, EventArgs.Empty);
			}
		}

#if DEBUG
		protected virtual
#endif
		Journal[] ProcessFile(string fileName, ZDateTime postDate)
		{
			var createdJournals = Array.Empty<Journal>();
			var journalImporterFactories = new List<BusinessObjectFactory>();

			using (var reader = new StreamReader(fileName))
			{
				reader.ReadLine(); //Line Header

				var importer = new JournalImporter(Ledger, postDate, ClearingAccount);
				journalImporterFactories.Add(importer.Factory);

				var count = 1;
				var currentRow = 0;
				var totalAmount = ZDecimal.Zero;

				while (count <= MaxNumberOfJournalsPerFactory && (CurrentLine = reader.ReadLine()) != null)
				{
					currentRow++;
					var journal = importer.AddJournalLine(CurrentLine);
					if (!importer.BlankLine)
					{
						if (importer.IsValid)
						{
							if (count == MaxNumberOfJournalsPerFactory)
							{
								importer = new JournalImporter(Ledger, postDate, ClearingAccount);
								journalImporterFactories.Add(importer.Factory);
							}
							var multiplier = journal.AH_Ledger == LedgerTypes.AccountsPayable ? -1 : 1;
							totalAmount += multiplier * journal.AH_InvoiceAmount;

							var arrayList = new ArrayList(createdJournals);
							arrayList.Add(journal);
							createdJournals = (Journal[])arrayList.ToArray(typeof(Journal));
						}
						else
						{
							DisplayFormatErrorMessage(importer.Errors, currentRow);
						}
					}
					if (ProgressChanged != null)
					{
						ProgressChanged(new ConverterData(currentRow, Errors, totalAmount), EventArgs.Empty);
					}
				}

				if (Errors.Count == 0)
				{
					BusinessObjectFactoryList.AddRange(journalImporterFactories);
				}
			}

			return createdJournals;
		}

#if DEBUG
		protected virtual
#endif
 bool ValidateClearingAccount()
		{
			if (ClearingAccount.IsEmpty)
			{
				Errors.Add(EmptyClearingAccountMessage());
			}
			return Errors.Count == 0;
		}

#if DEBUG
		protected virtual
#endif
 bool ValidateFile()
		{
			if (string.IsNullOrEmpty(FileName))
			{
				ProcessError(Res.GetString("45199a50-09e2-4bcd-851b-8d2a249278ef", "Please enter the file location"));
			}
			else if (!File.Exists(FileName))
			{
				ProcessError(Res.GetString("e80590a5-19fc-4791-9787-cb3194f2d6f9", "File {0} doesn't exist!", FileName));
			}
			else if (PostDate == ZDateTime.Empty || !PostDate.IsValid)
			{
				ProcessError(Res.GetString("cd6a8e0c-afe9-4599-89ef-56447ad576e5", "Please enter a valid Post Date"));
			}

			if (Errors.Count == 0)
			{
				try
				{
					using (StreamReader reader = new StreamReader(FileName))
					{
						OCsvLine firstLine = new OCsvLine(reader.ReadLine());
						if (!IsFileHeaderValid(firstLine))
						{
							ProcessError(InvalidHeaderMessage());
						}
					}
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					ProcessError(e.Message);
				}
			}

			return Errors.Count == 0;
		}

		void ProcessError(string errorMsg)
		{
			if (ErrorOccurred != null)
			{
				ErrorOccurred(errorMsg, EventArgs.Empty);
			}
			Errors.Add(errorMsg);
		}

		public ZString ValidateAccGLHeader(bool debtorsRadioButtonChecked)
		{
			ZString result = ZString.Empty;
			ZGuid glHeaderPK = (debtorsRadioButtonChecked ? (Guid)AccountingConfigurationRegistry.Instance.ARJournalAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)
				: (Guid)AccountingConfigurationRegistry.Instance.APJournalAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			AccGLHeader glHeader = Factory.Load<AccGLHeader>(glHeaderPK);

			if (glHeaderPK.IsEmpty)
			{
				result = @"You cannot import without the following GL Accounts in the registry:
Accounting->General Ledger Defaults->Link Account->AR Journal Account
Accounting->General Ledger Defaults->Link Account->AP Journal Account";
			}
			else if (glHeader == null || !glHeader.AG_IsActive)
			{
				result = @"The following registry settings need to have a valid and active GLHeader:
Accounting->General Ledger Defaults->Link Account->AR Journal Account
Accounting->General Ledger Defaults->Link Account->AP Journal Account";
			}
			return result;
		}

		BusinessObjectFactory Factory
		{
			get { return fFactory ?? (fFactory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory fFactory;

		#region Implementation

		const int MaxNumberOfJournalsPerFactory = 100;

		internal readonly StringCollection Errors;
		internal readonly ArrayList BusinessObjectFactoryList;

		protected bool IsStop;
		string FileName;
		internal string Ledger;
		string CurrentLine;

		ZGuid ClearingAccount;
		ZDateTime PostDate;

		protected void DisplayFormatErrorMessage(string detailedExceptionMessage, int rowNumber)
		{
			string rowData = Res.GetString("5dff66c4-bc14-487f-8721-13c46762076b", "Line {0}{1}{2}", rowNumber, CsvJournalConverter.Tab, CurrentLine);
			string errorMessage = rowData + CsvJournalConverter.Tab + detailedExceptionMessage;
			ProcessError(errorMessage);
		}

		string EmptyClearingAccountMessage()
		{
			return Res.GetString("71aa1845-06b2-450d-84f0-e9e467e510f9", "The {0} Journal Clearing Account cannot be empty.  Please enter GL Account for this in the registry", Ledger);
		}

		#region Get Methods

		BusinessObjectFactory[] JournalFactories
		{
			get
			{
				return (BusinessObjectFactory[])BusinessObjectFactoryList.ToArray(typeof(BusinessObjectFactory));
			}
		}

		#endregion

		#endregion
	}
}

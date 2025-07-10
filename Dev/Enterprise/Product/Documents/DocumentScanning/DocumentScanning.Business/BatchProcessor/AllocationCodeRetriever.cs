using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentScanning.Business
{
	/// <summary>
	/// Retrieves allocation information from an email subject line or filename, including ReferenceType, 
	/// Document Type and Code for business object to allocate to.
	/// </summary>
	public class AllocationCodeRetriever
	{
		public AllocationCodeRetriever(DocumentFactory factory, string emailSubjectLineOrFilename)
			: this(factory, emailSubjectLineOrFilename, false)
		{
		}

		public AllocationCodeRetriever(DocumentFactory factory, string emailSubjectLineOrFilename, bool throwExceptionOnInvalidPK)
		{
			masterFactory = factory;
			VisibleInfo = new VisibleCompanyBranchDepartmentInfo();
			allocationInfo = new AllocationInfo(emailSubjectLineOrFilename);
			ProcessAllocationInformation(throwExceptionOnInvalidPK);
		}

		readonly DocumentFactory masterFactory;
		readonly AllocationInfo allocationInfo;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception message")]
		void ProcessAllocationInformation(bool throwExceptionOnInvalidPK)
		{
			CheckLengths();

			if (!AssemblyDataLookup.IsDocManagerCodeValid(RefType))
			{
				RefType = ZString.Empty;
			}

			var errorMessage = string.Empty;

			if (!RefCode.IsEmpty && !RefType.IsEmpty)
			{
				var recordCompanyCode = allocationInfo.RecordCompanyCode;
				if (!string.IsNullOrWhiteSpace(recordCompanyCode))
				{
					refObject = GetBusinessObjectFromCodeWithinCompany();
					if (refObject == null)
					{
						refObject = GetBusinessObjectFromCode();
					}
				}
				else
				{
					refObject = GetBusinessObjectFromCode();
				}

				if (throwExceptionOnInvalidPK && refObject == null)
				{
					errorMessage = string.Format("The unique ID you have supplied ({0}) is not valid for Reference Type {1}. " +
						"Check that you have used the correct unique ID for allocating these documents.", RefCode, RefType);
					if (string.IsNullOrWhiteSpace(recordCompanyCode))
					{
						errorMessage += " If the unique ID exists in multiple records across different companies you may need to also specify the company code to properly identify the record.";
					}
				}
			}

			string visibleCodeErrorMessage = string.Empty;
			allocationInfo.SetVisibleCompanyPK(VisibleInfo, masterFactory, ref visibleCodeErrorMessage);
			allocationInfo.SetVisibleBranchPK(VisibleInfo, masterFactory, ref visibleCodeErrorMessage);
			allocationInfo.SetVisibleDepartmentPK(VisibleInfo, masterFactory, ref visibleCodeErrorMessage);

			if (!string.IsNullOrEmpty(visibleCodeErrorMessage) && throwExceptionOnInvalidPK)
			{
				throw new IncorrectVisibleCompanyBranchDepartmentException(visibleCodeErrorMessage);
			}

			if (!string.IsNullOrEmpty(errorMessage))
			{
				throw new AllocationCodeFormatException(errorMessage);
			}
		}

		BusinessObject GetBusinessObjectFromCodeWithinCompany()
		{
			return AssemblyDataLookup.GetBusinessObjectFromCode(masterFactory, RefType, RefCode, allocationInfo.RecordCompanyCode, true);
		}

		BusinessObject GetBusinessObjectFromCode()
		{
			return AssemblyDataLookup.GetBusinessObjectFromCode(masterFactory, RefType, RefCode, true);
		}

		void CheckLengths()
		{
			if (RefType.Length > StorageMainSchema.SM_Type.MaxLength || DocType.Length > StorageDocsSchema.SC_DocType.MaxLength)
			{
				throw new AllocationCodeFormatException(
					string.Format(CultureInfo.InvariantCulture, "Verify that the allocation details are correct. The correct format for the allocation details are:" +
					"\t[DocManager (3-letter Reference) (3 or 4 letters DocType) (Unique ID)]" + System.Environment.NewLine +
					"\tFor example, [DocManager SHP PAL S0000100]" + System.Environment.NewLine +
					"But they were (Reference code: {0}), (DocType: {1}), (Unique ID: {2})", RefType, DocType, RefCode));
			}
		}

		public ZString RefType
		{
			get { return allocationInfo.RefType; }
			set { allocationInfo.RefType = value; }
		}

		public ZString DocType
		{
			get { return allocationInfo.DocType; }
		}

		public ZString DocSource
		{
			get { return allocationInfo.DocSource; }
		}

		ZString RefCode
		{
			get { return allocationInfo.RefCode; }
		}

		public BusinessObject RefObject => refObject;
		BusinessObject refObject;

		public ZGuid RefPK => refObject?.PK ?? ZGuid.Empty;

#if DEBUG
		public
#else
		internal
#endif
		VisibleCompanyBranchDepartmentInfo VisibleInfo { get; private set; }
	}
}

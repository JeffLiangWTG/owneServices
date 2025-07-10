using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class RequiredDocumentsWrapperCollection : GenericWrapperCollection<RequiredDocumentsWrapper>
	{
		public RequiredDocumentsWrapperCollection(BusinessObjectFactory factory)
			: base(factory) { }

		public RequiredDocumentsWrapperCollection(JobRequiredDocumentDependentCollection collectionSource, FreightWrapper[] additionalJobs, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
			this.AdditionalJobs = additionalJobs;
		}

		public RequiredDocumentsWrapperCollection(JobRequiredDocumentDependentCollection collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap) { }

		readonly FreightWrapper[] AdditionalJobs;

		#region MissingRequiredDocuments

		public ZString MissingRequiredDocuments
		{
			get
			{
				ZString result = ZString.Empty;
				Sort("Type", ListSortDirection.Ascending);
				foreach (RequiredDocumentsWrapper doc in this)
				{
					result += FormatMissingDocument(doc);
				}

				if (AdditionalJobs != null)
				{
					foreach (FreightWrapper additionalJob in AdditionalJobs)
					{
						result += additionalJob.JobNumberHeading + ": " + additionalJob.JobNumber + System.Environment.NewLine;
						result += additionalJob.RequiredDocuments.MissingRequiredDocuments + System.Environment.NewLine;
					}
				}

				return result.TrimEnd();
			}
		}

		ZString FormatMissingDocument(RequiredDocumentsWrapper doc)
		{
			ZString result = "";

			if (doc != null && !doc.IsReceived && !doc.Description.IsEmpty && doc.IsCorrectUsage)
			{
				result = "- " + doc.Description;
				result += (!doc.IsOriginalRequired) ? System.Environment.NewLine : " " + Res.GetString("1e3dbaff-7442-484b-ad18-fa49c53c64af", "- Original Required") + System.Environment.NewLine;
			}

			return result;
		}

		#endregion
	}
}

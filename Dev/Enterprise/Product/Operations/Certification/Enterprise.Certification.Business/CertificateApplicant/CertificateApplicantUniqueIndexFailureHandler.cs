using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Certification.Business
{
	class CertificateApplicantUniqueIndexFailureHandler : IUniqueIndexFailureHandler
	{
		public CertificateApplicantUniqueIndexFailureHandler(CertificateApplicant applicant)
		{
			this.applicant = applicant;
		}

		#region IUniqueIndexFailureHandler Members

		public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			ZQuery query = new ZQuery(HRJobApplicantSchema.HA_EmailAddress, applicant.HA_EmailAddress);
			query.AddToFilter(HRJobApplicantSchema.PK, SQLComparisonOperator.NotEqual, applicant.PK);
			CertificateApplicant existingApplicant = factory.LoadTop1<CertificateApplicant>(query);
			if (existingApplicant != null)
			{
				notifier.ReportInformation(
					ApprovedCertificateApplicantCreator.GetCannotCreateDuplicateUserMessage(applicant.HA_FullName, applicant.HA_EmailAddress, existingApplicant),
					ApprovedCertificateApplicantCreator.CannotCreateDuplicateUserCaption);
			}
		}

		public IEnumerable<string> HandledUniqueIndexNames
		{
			get { yield return HRJobApplicantSchema.Constants.Indexes.NR_UX__HA_EmailAddress; }
		}

		#endregion

		readonly CertificateApplicant applicant;
	}
}

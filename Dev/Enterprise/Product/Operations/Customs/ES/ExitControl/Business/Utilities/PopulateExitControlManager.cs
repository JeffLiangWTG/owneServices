using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.ExitControl.Business
{
	public class PopulateExitControlManager : IPopulateExitControlManager
	{
		public PopulateExitControlData AddOrUpdateExitControl(JobDeclaration declaration, IEnumerable<CusEntryHeader> acceptedEntries, Func<ZString, bool> updateConfirmation)
		{
			var populateData = new PopulateExitControlData();

			var populateExitControlHelper = new PopulateExitControlHelper();

			var (exitHeaders, mrnsExistingButNotAssociatedList, mrnsExistingButNotAssociatedErrorMessageList) = populateExitControlHelper.GetOrCreateExitHeader(declaration, acceptedEntries);

			populateData.ErrorMessages = mrnsExistingButNotAssociatedErrorMessageList;

			var entriesWitHeaderAssociated = acceptedEntries.Except(acceptedEntries.Where(x => mrnsExistingButNotAssociatedList.Contains(x.MovementReferenceNumber)));

			foreach (var exitHeader in exitHeaders)
			{
				IEnumerable<CusEntryHeader> updatableEntries = null;
				IEnumerable<CusEntryHeader> entriesToAddOrUpdate = null;

				if (exitHeader.CusExitConsignments.Count > 0)
				{
					(updatableEntries, populateData.NonUpdatableEntriesMRNs) = RemoveNonUpdatableEntries(declaration, entriesWitHeaderAssociated, exitHeader.CusExitReports);

					entriesToAddOrUpdate = GetConfirmationToUpdateEntries(updatableEntries, exitHeader, updateConfirmation);
				}
				else
				{
					entriesToAddOrUpdate = entriesWitHeaderAssociated;
				}

				populateData.ObjectsAddedOrUpdated = populateExitControlHelper.GenerateExitControlFromEntries(declaration.GetMRNAndReferenceFromEntries(entriesToAddOrUpdate), exitHeader, declaration);
			}
			return populateData;
		}

		(IEnumerable<CusEntryHeader>, IEnumerable<ZString>) RemoveNonUpdatableEntries(JobDeclaration declaration, IEnumerable<CusEntryHeader> acceptedEntries, ICusExitReportCollection<CusExitReport> exitReports)
		{
			var nonUpdatableEntries = new PopulateExitControlHelper().GetEntriesWithMRNInAcceptedExitConsignments(acceptedEntries, exitReports);
			var updatableEntries = acceptedEntries.Except(nonUpdatableEntries);
			return (updatableEntries, declaration.GetMRNFromEntries(nonUpdatableEntries));
		}

		IEnumerable<CusEntryHeader> GetConfirmationToUpdateEntries(IEnumerable<CusEntryHeader> acceptedEntries, CusExitHeader exitHeader, Func<ZString, bool> updateConfirmation)
		{
			var updatableEntries = new PopulateExitControlHelper().GetEntriesWithMRNInNotAcceptedExitConsignments(acceptedEntries, exitHeader);
			var entriesToAddOrUpdate = acceptedEntries.Except(updatableEntries).ToList();

			foreach (var entry in updatableEntries)
			{
				if (updateConfirmation(entry.MovementReferenceNumber))
				{
					entriesToAddOrUpdate.Add(entry);
				}
			}
			return entriesToAddOrUpdate;
		}
	}
}

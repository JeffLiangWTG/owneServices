using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.Business
{
	public interface IPopulateExitControlManager
	{
		PopulateExitControlData AddOrUpdateExitControl(JobDeclaration declaration, IEnumerable<CusEntryHeader> acceptedEntries, Func<ZString, bool> updateConfirmation);
	}

	public class PopulateExitControlData
	{
		public int ObjectsAddedOrUpdated { get; set; }
		public IEnumerable<ZString> NonUpdatableEntriesMRNs { get; set; }
		public IEnumerable<ZString> ErrorMessages { get; set; }
	}
}

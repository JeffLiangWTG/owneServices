using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.ManifestBase
{
	public interface IAsycudaFetchStrategy
	{
		IEnumerable<BusinessObject> FetchForDelete();
		void AddFetchHintsForDelete();

		IEnumerable<BusinessObject> FetchForLoadChildEditableObjects();
		void AddFetchHintsForLoadChildEditableObjects();

		IEnumerable<BusinessObject> FetchForValidate();
		void AddFetchHintsForValidate();
	}
}

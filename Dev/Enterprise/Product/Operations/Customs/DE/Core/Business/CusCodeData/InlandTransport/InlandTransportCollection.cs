using System;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Business
{
	public sealed class InlandTransportCollection : EU.Business.Declaration.InlandTransportCollection
	{
		public InlandTransportCollection(Declaration.JobDeclaration master)
			: base(master)
		{
			SetMaxCountValidation();
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pk) => typeof(InlandTransport);

		public new InlandTransport AddNew() => (InlandTransport)base.AddNew();

		public new InlandTransport AddNew(Type bizOType) => (InlandTransport)base.AddNew(bizOType);

		public new InlandTransport this[int index] => (InlandTransport)base[index];

		void SetMaxCountValidation()
		{
			var maxCountForValidation = 999;
			MaxCountValidationEnable(maxCountForValidation, MaxCountErrorMessage(maxCountForValidation));
		}

		string MaxCountErrorMessage(int maxCount) => Res.GetString("1809032E-9D15-4529-AB31-588E06FF3B13", "You may enter a maximum of {0} Wagon Numbers here.", maxCount);
	}
}

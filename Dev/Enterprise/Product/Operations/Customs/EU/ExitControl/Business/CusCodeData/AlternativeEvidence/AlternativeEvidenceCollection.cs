using System.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public interface IAlternativeEvidenceCollection<out T> : Customs.Business.ICusCodeDataCollection<T>, IBindingList
		where T : AlternativeEvidence
	{
		new T this[int index] { get; }
		new T AddNew();
	}

	public class AlternativeEvidenceCollection<T> : Customs.Business.CusCodeDataCollection<T>, IAlternativeEvidenceCollection<T>
		where T : AlternativeEvidence
	{
		const int maxCount = 9;

		public AlternativeEvidenceCollection(BusinessObject parent) : base(parent, EU.Business.CusCodeDataTypeList.Codes.AlternativeEvidence)
		{
			this.EnableMaxCountValidation(maxCount, Res.GetString("56AE817E-EB29-4471-8ED5-639D695D0641", "Maximum number of items is {0}.", maxCount), false);
		}

		protected override bool AllowNewCore => Count < maxCount;
	}
}

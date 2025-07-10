using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface INctsBillAdditionalDocumentCollection<out T> : ICusSupportingInfoCollection<T>, ISequenceNumberHeader, INctsAdditionalInfoSequenceHeader
	where T : NctsBillAdditionalDocument
	{
		new T this[int index] { get; }
		new T AddNew();
	}

	public class NctsBillAdditionalDocumentCollection<T> : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection<T>, INctsBillAdditionalDocumentCollection<T>
	where T : NctsBillAdditionalDocument
	{
		public NctsBillAdditionalDocumentCollection(NctsBill parent) : base(parent)
		{
		}

		public IEnumerable<ISequenceNumberLine> Lines => new TypedEnumerable<ISequenceNumberLine>(Elements);

		public new IEnumerator<T> GetEnumerator() => Elements.Cast<T>().GetEnumerator();

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var additionalDocument = (T)child;
			additionalDocument.CSI_Status = NctsBillAdditionalDocumentStatusList.Codes.NEW;
		}

		public ShortSequenceNumberGenerator RefSequenceNumberGenerator
		{
			get
			{
				return refSequenceNumberCalculator ?? (refSequenceNumberCalculator = new ShortSequenceNumberGenerator(this, (x) => ((T)x).CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference));
			}
		}
		ShortSequenceNumberGenerator refSequenceNumberCalculator;

		public ShortSequenceNumberGenerator InfSequenceNumberGenerator
		{
			get
			{
				return infSequenceNumberCalculator ?? (infSequenceNumberCalculator = new ShortSequenceNumberGenerator(this, (x) => ((T)x).CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation));
			}
		}
		ShortSequenceNumberGenerator infSequenceNumberCalculator;

		public ShortSequenceNumberGenerator TraSequenceNumberGenerator
		{
			get
			{
				return traSequenceNumberCalculator ?? (traSequenceNumberCalculator = new ShortSequenceNumberGenerator(this, (x) => ((T)x).CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument));
			}
		}

		ShortSequenceNumberGenerator traSequenceNumberCalculator;
	}
}

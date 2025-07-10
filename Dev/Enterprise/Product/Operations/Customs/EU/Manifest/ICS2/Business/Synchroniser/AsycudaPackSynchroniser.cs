using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class AsycudaPackSynchroniser : ASYCUDA.Business.AsycudaPackSynchroniser
	{
		public AsycudaPackSynchroniser(AsycudaPack destination, PackLine source) : base(destination, source)
		{
		}

		protected new AsycudaPack Destination => (AsycudaPack)base.Destination;

		IZType GetMarksAndNumbers()
		{
			var marksAndNumbers = Source.JL_MarksAndNumbers.IsEmpty ? Source.Shipment.JS_MarksAndNumbers : Source.JL_MarksAndNumbers;

			marksAndNumbers = Regex.Replace(marksAndNumbers, @"[\r\n]", " ");
			marksAndNumbers = Regex.Replace(marksAndNumbers.Trim(), @"\s+", " ");

			return marksAndNumbers;
		}

		protected ZPropertyInfo[] GetSourceInfosAffectingMarksAndNumbers() => new[] { Source.JL_MarksAndNumbers.IsEmpty ? Source.Shipment.JS_MarksAndNumbersInfo : Source.JL_MarksAndNumbersInfo };

		protected override void SynchronisersAMA_MarksAndNumbers()
		{
			Synchronisers.Add(new FieldSynchroniser(Destination.APA_MarksAndNumbersInfo, GetMarksAndNumbers, GetSourceInfosAffectingMarksAndNumbers));
		}
	}
}

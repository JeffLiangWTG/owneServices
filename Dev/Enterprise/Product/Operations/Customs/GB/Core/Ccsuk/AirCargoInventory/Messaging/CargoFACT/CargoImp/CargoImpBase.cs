using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Integration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public abstract class CargoImpBase
	{
		public CargoImpBase(ErrorCollector ec)
		{
			this.errorCollector = ec;
		}
		public abstract ZString CargoImpCode { get; }
		protected abstract string[] CargoImpLinesWithoutType { get; }
		public abstract ZString MessageInterpretation { get; }

		public string[] AllCargoImpLinesIncludingType
		{
			get
			{
				var result = new List<string>();
				var optionalVersion = "";
				if (CargoImpVersion > 0)
				{
					optionalVersion = slash + CargoImpVersion.ToString();
				}
				result.Add(CargoImpCode + optionalVersion);
				result.AddRange(CargoImpLinesWithoutType);
				return result.ToArray();
			}
		}

		protected void DemandFieldsNotEmpty(params ZString[] parameters)
		{
			for (var i = 0; i < parameters.Length; i += 2)
			{
				if (parameters[i + 1].IsEmpty)
				{
					errorCollector.AddError(parameters[i], new ErrorInfo("", "mandatory"));
				}
			}
		}

		public ZInt CargoImpVersion
		{
			get { return CargoImpVersionCore; }
		}

		protected virtual ZInt CargoImpVersionCore
		{
			get { return 0; }
		}

		internal static ICcsukCusAwb FindAwb(string mawpAndMawnAndHawb, string splitNumber, string airportAndShed, BusinessObjectFactory factory)
		{
			// mawpAndMawnAndHawb looks like 125-12345678 or 125-12345678-87654321
			try
			{
				var parts = Regex.Split(mawpAndMawnAndHawb, "-");
				var mawp = NthElementOrEmpty(0, parts);
				var mawn = NthElementOrEmpty(1, parts);
				var hawb = NthElementOrEmpty(2, parts);

				ICcsukCusAwb awb = null;
				if (!String.IsNullOrEmpty(hawb))
				{
					awb = new CusHAWB.Loader(factory).FindHawb(hawb, splitNumber, mawp + mawn, airportAndShed);
				}
				else if (!String.IsNullOrEmpty(mawn))
				{
					awb = new CusMAWB.Loader(factory).FindFromMawbNumber(mawp + mawn, splitNumber, airportAndShed);
				}

				if (awb != null && awb.HasSplits)
				{
					var split = awb.Splits[splitNumber];
					if (split != null)
					{
						awb = split;
					}
				}

				return awb;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				throw new FormatException("Could not parse consignment ID, format was unexpected. " + mawpAndMawnAndHawb, ex);
			}
		}

		internal static string NthElementOrEmpty(int index, string[] elements)
		{
			return elements.Length > index ? elements[index] : "";
		}

		protected EDIMessage TryLoadOutboundMessageFromCommonAccessReference(ZString car, BusinessObjectFactory factory)
		{
			if (!car.IsEmpty)
			{
				try
				{
					var guid = new ZGuid(new Guid(car));
					return factory.Load<EDIMessage>(guid);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{ }
			}
			return null;
		}

		protected const string slash = "/";
		protected ErrorCollector errorCollector;
		public ILogger ServiceLogger { get; set; }
		protected void Log(LogType type, string message)
		{
			if (ServiceLogger != null)
			{
				ServiceLogger.Log(type, message);
			}
		}
	}
}

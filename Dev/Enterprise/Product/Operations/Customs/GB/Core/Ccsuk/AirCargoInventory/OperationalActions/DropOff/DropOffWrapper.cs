using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.OperationalActions
{
	public class DropOffWrapper
	{
		public DropOffWrapper()
		{
			Header = new DropOffHeader();
			ShedGroups = new List<DropOffShedGroup>();
			Footer = new DropOffFooter();
		}

		public DropOffHeader Header { get; private set; }

		public List<DropOffShedGroup> ShedGroups { get; private set; }

		public DropOffFooter Footer;

		public override string ToString()
		{
			var sb = new ZStringBuilder();
			sb.Append(Header.ToString());
			foreach (var s in ShedGroups)
			{
				sb.Append(s.ToString());
			}
			sb.Append(Footer.ToString());
			return sb.ToStringWithNewLineBetweenAppends();
		}
	}

	public class DropOffHeader
	{
		public ZString Agent { get; set; }

		public ZString Airport { get; set; }

		public ZString Vehicle { get; set; }

		public ZDateTime ETA { get; set; }

		public ZDateTime ETD { get; set; }

		public override string ToString()
		{
			return string.Format("1/ZZ{0}/{1}/{2}/{3}/GB/{4}/{5}", RotationNumberPlaceholder, ETD.ToString("ddMMMHHmm"), Agent, Vehicle, ETA.ToString("ddMMMHHmm"), Airport);
		}

		public const string RotationNumberPlaceholder = "<<ROTATIONNUMBER>>";
	}

	public class DropOffShedGroup
	{
		public DropOffShedGroup()
		{
			UldLines = new List<DropOffUldGroup>();
		}

		public string Shed { get; set; }

		public List<DropOffUldGroup> UldLines { get; set; }

		public override string ToString()
		{
			var sb = new ZStringBuilder();
			foreach (var u in UldLines)
			{
				sb.Append(u.ToString());
			}
			sb.Prepend(Shed);
			return sb.ToStringWithNewLineBetweenAppends();
		}
	}

	public class DropOffAwb
	{
		public ZString AwbNumber { get; set; }

		public ZString Description { get; set; }

		public ZWeight Weight { get; set; }

		public ZVolume Volume { get; set; }

		public ZInt TotalPackages { get; set; }

		public ZInt CurrentPackages { get; set; }

		public RefUNLOCO UnlocoOrigin { get; set; }

		public RefUNLOCO UnlocoDestination { get; set; }

		public override string ToString()
		{
			var firstPieceCount = "";
			var secondPieceCount = "";
			if (TotalPackages == CurrentPackages || CurrentPackages == 0)
			{
				firstPieceCount = "T" + TotalPackages.ToString();
			}
			else
			{
				firstPieceCount = "S" + CurrentPackages.ToString();
				secondPieceCount = "T" + TotalPackages.ToString();
			}
			var origin = UnlocoOrigin != null ? UnlocoOrigin.RL_IATA : ZString.Empty;
			var dest = UnlocoDestination != null ? UnlocoDestination.RL_IATA : ZString.Empty;
			return string.Format("{0}-{1}{2}{3}/{4}K{5}{6}MC{7}/{8}", AwbNumber.Left(3),  // 0
															AwbNumber.Right(8), //1
															origin, //2
															dest, //3
															firstPieceCount, //4 
															Weight.InKilograms, //5
															secondPieceCount, //6
															Volume.InCubicMetres,//7
															Description.Left(15)); // 8
		}
	}

	public class DropOffUldGroup
	{
		public DropOffUldGroup()
		{
			AwbLines = new List<DropOffAwb>();
		}

		public ZString UldNumber { get; set; }

		public List<DropOffAwb> AwbLines { get; set; }

		public bool IsLoose
		{
			get { return UldNumber.IsEmpty; }
		}

		public override string ToString()
		{
			var sb = new ZStringBuilder();
			if (!IsLoose)
			{
				sb.Append("ULD/" + UldNumber);
			}
			foreach (var a in AwbLines)
			{
				sb.Append(a.ToString());
			}
			return sb.ToStringWithNewLineBetweenAppends();
		}
	}

	public class DropOffFooter
	{
		public override string ToString()
		{
			return "LAST";
		}
	}
}

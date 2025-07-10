
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.STI.Navision
{
	public class JobFlatFileDataRow : NavisionFlatFileDataRow
	{
		public JobFlatFileDataRow() : base(JobFlatFileDataRow.NumberOfFields)
		{
		}

		#region Schema

		class Schema
		{
			public static readonly FlatFileFieldProperty JobNumber = new FlatFileFieldProperty(0, 20);
			public static readonly FlatFileFieldProperty SearchDescription = new FlatFileFieldProperty(1, 50);
			public static readonly FlatFileFieldProperty Description = new FlatFileFieldProperty(2, 50);
			public static readonly FlatFileFieldProperty Description2 = new FlatFileFieldProperty(3, 50);
			public static readonly FlatFileFieldProperty BillToCustomerNumber = new FlatFileFieldProperty(4, 20);
			public static readonly FlatFileFieldProperty CreationDate = new FlatFileFieldProperty(5, 8);
			public static readonly FlatFileFieldProperty StartingDate = new FlatFileFieldProperty(6, 8);
			public static readonly FlatFileFieldProperty EndingDate = new FlatFileFieldProperty(7, 8);
			public static readonly FlatFileFieldProperty JobPostingDate = new FlatFileFieldProperty(8, 8);
			public static readonly FlatFileFieldProperty CompletionPercentage = new FlatFileFieldProperty(9, 1);
			public static readonly FlatFileFieldProperty Status = new FlatFileFieldProperty(10, 5);
			public static readonly FlatFileFieldProperty PersonResponsible = new FlatFileFieldProperty(11, 20);
			public static readonly FlatFileFieldProperty GlobalDimension1Code = new FlatFileFieldProperty(12, 20);
			public static readonly FlatFileFieldProperty GlobalDimension2Code = new FlatFileFieldProperty(13, 20);
			public static readonly FlatFileFieldProperty JobPostingGroup = new FlatFileFieldProperty(14, 10);
			public static readonly FlatFileFieldProperty RecognitionMethod = new FlatFileFieldProperty(15, 20);
			public static readonly FlatFileFieldProperty ApplicationMethod = new FlatFileFieldProperty(16, 6);
			public static readonly FlatFileFieldProperty JobUsagePosting = new FlatFileFieldProperty(17, 4);
			public static readonly FlatFileFieldProperty OwnerReference = new FlatFileFieldProperty(18, 20);
			public static readonly FlatFileFieldProperty ConsignorName = new FlatFileFieldProperty(19, 50);
			public static readonly FlatFileFieldProperty MasterAWB = new FlatFileFieldProperty(20, 20);
			public static readonly FlatFileFieldProperty HouseAWB = new FlatFileFieldProperty(21, 20);
			public static readonly FlatFileFieldProperty VesselFlight = new FlatFileFieldProperty(22, 20);
			public static readonly FlatFileFieldProperty TransportMode = new FlatFileFieldProperty(23, 10);
			public static readonly FlatFileFieldProperty ContainerNumbers = new FlatFileFieldProperty(24, 250);
			public static readonly FlatFileFieldProperty LoadingPort = new FlatFileFieldProperty(25, 20);
			public static readonly FlatFileFieldProperty DischargePort = new FlatFileFieldProperty(26, 20);
			public static readonly FlatFileFieldProperty ConsolidatedETA = new FlatFileFieldProperty(27, 8);
			public static readonly FlatFileFieldProperty ConsolidatedETD = new FlatFileFieldProperty(28, 8);
			public static readonly FlatFileFieldProperty GoodsDescription = new FlatFileFieldProperty(29, 250);
		}

		#endregion

		#region Properties

		public ZString JobNumber
		{
			get { return this[Schema.JobNumber.Name]; }
			set { SetField(Schema.JobNumber, value, true); }
		}

		public ZString SearchDescription
		{
			get { return this[Schema.SearchDescription.Name]; }
			set { SetField(Schema.SearchDescription, value); }
		}

		public ZString Description
		{
			get { return this[Schema.Description.Name]; }
			set { SetField(Schema.Description, value); }
		}

		public ZString Description2
		{
			get { return this[Schema.Description2.Name]; }
			set { SetField(Schema.Description2, value); }
		}

		public ZString BillToCustomerNumber
		{
			get { return this[Schema.BillToCustomerNumber.Name]; }
			set { SetField(Schema.BillToCustomerNumber, value); }
		}

		public ZDateTime CreationDate
		{
			get { return GetFieldAsZDateTime(Schema.CreationDate.Name, Constants.DateTimeFormat); }
			set { SetField(Schema.CreationDate, value); }
		}

		public ZDateTime StartingDate
		{
			get { return GetFieldAsZDateTime(Schema.StartingDate.Name, Constants.DateTimeFormat); }
			set { SetField(Schema.StartingDate, value); }
		}

		public ZDateTime EndingDate
		{
			get { return GetFieldAsZDateTime(Schema.EndingDate.Name, Constants.DateTimeFormat); }
			set { SetField(Schema.EndingDate, value); }
		}

		public ZDateTime JobPostingDate
		{
			get { return GetFieldAsZDateTime(Schema.JobPostingDate.Name, Constants.DateTimeFormat); }
			set { SetField(Schema.JobPostingDate, value); }
		}

		public ZInt CompletionPercentage
		{
			get { return GetFieldAsZInt(Schema.CompletionPercentage.Name); }
			set { SetField(Schema.CompletionPercentage, value); }
		}

		public ZString Status
		{
			get { return this[Schema.Status.Name]; }
			set { SetField(Schema.Status, value); }
		}

		public ZString PersonResponsible
		{
			get { return this[Schema.PersonResponsible.Name]; }
			set { SetField(Schema.PersonResponsible, value); }
		}

		public ZString GlobalDimension1Code
		{
			get { return this[Schema.GlobalDimension1Code.Name]; }
			set { SetField(Schema.GlobalDimension1Code, value); }
		}

		public ZString GlobalDimension2Code
		{
			get { return this[Schema.GlobalDimension2Code.Name]; }
			set { SetField(Schema.GlobalDimension2Code, value); }
		}

		public ZString JobPostingGroup
		{
			get { return this[Schema.JobPostingGroup.Name]; }
			set { SetField(Schema.JobPostingGroup, value); }
		}

		public ZString RecognitionMethod
		{
			get { return this[Schema.RecognitionMethod.Name]; }
			set { SetField(Schema.RecognitionMethod, value); }
		}

		public ZString ApplicationMethod
		{
			get { return this[Schema.ApplicationMethod.Name]; }
			set { SetField(Schema.ApplicationMethod, value); }
		}

		public ZString JobUsagePosting
		{
			get { return this[Schema.JobUsagePosting.Name]; }
			set { SetField(Schema.JobUsagePosting, value); }
		}

		public ZString OwnerReference
		{
			get { return this[Schema.OwnerReference.Name]; }
			set { SetField(Schema.OwnerReference, value); }
		}

		public ZString ConsignorName
		{
			get { return this[Schema.ConsignorName.Name]; }
			set { SetField(Schema.ConsignorName, value); }
		}

		public ZString MasterAWB
		{
			get { return this[Schema.MasterAWB.Name]; }
			set { SetField(Schema.MasterAWB, value); }
		}

		public ZString HouseAWB
		{
			get { return this[Schema.HouseAWB.Name]; }
			set { SetField(Schema.HouseAWB, value); }
		}

		public ZString VesselFlight
		{
			get { return this[Schema.VesselFlight.Name]; }
			set { SetField(Schema.VesselFlight, value); }
		}

		public ZString TransportMode
		{
			get { return this[Schema.TransportMode.Name]; }
			set { SetField(Schema.TransportMode, value); }
		}

		public ZString ContainerNumbers
		{
			get { return this[Schema.ContainerNumbers.Name]; }
			set { SetField(Schema.ContainerNumbers, value); }
		}

		public ZString LoadingPort
		{
			get { return this[Schema.LoadingPort.Name]; }
			set { SetField(Schema.LoadingPort, value); }
		}

		public ZString DischargePort
		{
			get { return this[Schema.DischargePort.Name]; }
			set { SetField(Schema.DischargePort, value); }
		}

		public ZDateTime ConsolidatedETA
		{
			get { return GetFieldAsZDateTime(Schema.ConsolidatedETA.Name, Constants.DateTimeFormat); }
			set { SetField(Schema.ConsolidatedETA, value); }
		}

		public ZDateTime ConsolidatedETD
		{
			get { return GetFieldAsZDateTime(Schema.ConsolidatedETD.Name, Constants.DateTimeFormat); }
			set { SetField(Schema.ConsolidatedETD, value); }
		}

		public ZString GoodsDescription
		{
			get { return this[Schema.GoodsDescription.Name]; }
			set { SetField(Schema.GoodsDescription, value.Replace("\r\n", " ")); }
		}

		#endregion

		const int NumberOfFields = 30;
	}
}

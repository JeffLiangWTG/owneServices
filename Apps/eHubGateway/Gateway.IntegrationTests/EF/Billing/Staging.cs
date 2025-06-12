namespace CargoWise.eHub.Gateway.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Staging")]
    public partial class Staging
    {
        [Key]
        [Column(Order = 0)]
        public long TX_ID { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(3)]
        public string TX_Category { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(3)]
        public string TX_PriceItemCode { get; set; }

        [Key]
        [Column(Order = 3)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TX_BillableCount { get; set; }

        [Key]
        [Column(Order = 4)]
        [StringLength(3)]
        public string TX_ReportingSource { get; set; }

        [Key]
        [Column(Order = 5, TypeName = "datetime2")]
        public DateTime TX_ServiceOccuredUTC { get; set; }

        [Key]
        [Column(Order = 6)]
        [StringLength(9)]
        public string TX_ClientID { get; set; }

        [StringLength(50)]
        public string TX_ClientNumber { get; set; }

        [StringLength(3)]
        public string TX_ClientStaffCode { get; set; }

        [Key]
        [Column(Order = 7)]
        [StringLength(50)]
        public string TX_Reference1 { get; set; }

        [StringLength(50)]
        public string TX_Reference2 { get; set; }

        [StringLength(50)]
        public string TX_Reference3 { get; set; }

        [StringLength(50)]
        public string TX_Reference4 { get; set; }

        [StringLength(50)]
        public string TX_Reference5 { get; set; }

        [Key]
        [Column(Order = 8, TypeName = "datetime2")]
        public DateTime TX_SystemCreateUTC { get; set; }

        [Key]
        [Column(Order = 9)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TX_Version { get; set; }

        [StringLength(3)]
        public string TX_Branch { get; set; }

        [StringLength(36)]
        public string TX_MessageTrackingID { get; set; }

        [Key]
        [Column(Order = 10)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TX_Period { get; set; }

        [Key]
        [Column(Order = 11)]
        public bool IsRefSwapped { get; set; }

        [Key]
        [Column(Order = 12)]
        public byte ProcessingStatus { get; set; }

        [Key]
        [Column(Order = 13)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int DatabaseNumber { get; set; }

        [Key]
        [Column(Order = 14)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short CompanyNumber { get; set; }
    }
}

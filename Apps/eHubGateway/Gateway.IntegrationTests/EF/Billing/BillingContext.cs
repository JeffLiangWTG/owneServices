using System.Data.Entity;

namespace CargoWise.eHub.Gateway.IntegrationTests
{
	public class BillingContext : DbContext
	{
		public BillingContext(string nameOrConnectionString)
			: base(nameOrConnectionString)
		{
			Database.SetInitializer(new CreateDatabaseIfNotExists<BillingContext>());
		}

		public virtual DbSet<Staging> Stagings { get; set; }

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Staging>()
				.Property(e => e.TX_Category)
				.IsUnicode(false);

			modelBuilder.Entity<Staging>()
				.Property(e => e.TX_PriceItemCode)
				.IsUnicode(false);

			modelBuilder.Entity<Staging>()
				.Property(e => e.TX_ReportingSource)
				.IsUnicode(false);

			modelBuilder.Entity<Staging>()
				.Property(e => e.TX_ClientID)
				.IsUnicode(false);

			modelBuilder.Entity<Staging>()
				.Property(e => e.TX_ClientNumber)
				.IsUnicode(false);

			modelBuilder.Entity<Staging>()
				.Property(e => e.TX_ClientStaffCode)
				.IsUnicode(false);

			modelBuilder.Entity<Staging>()
				.Property(e => e.TX_Reference1)
				.IsUnicode(false);

			modelBuilder.Entity<Staging>()
				.Property(e => e.TX_Reference2)
				.IsUnicode(false);

			modelBuilder.Entity<Staging>()
				.Property(e => e.TX_Reference3)
				.IsUnicode(false);

			modelBuilder.Entity<Staging>()
				.Property(e => e.TX_Reference4)
				.IsUnicode(false);

			modelBuilder.Entity<Staging>()
				.Property(e => e.TX_Reference5)
				.IsUnicode(false);

			modelBuilder.Entity<Staging>()
				.Property(e => e.TX_SystemCreateUTC)
				.HasPrecision(0);

			modelBuilder.Entity<Staging>()
				.Property(e => e.TX_Branch)
				.IsUnicode(false);

			modelBuilder.Entity<Staging>()
				.Property(e => e.TX_MessageTrackingID)
				.IsUnicode(false);
		}
	}
}

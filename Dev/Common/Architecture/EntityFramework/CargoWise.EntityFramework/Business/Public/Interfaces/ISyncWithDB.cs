namespace CargoWise.EntityFramework
{
	public interface ISyncWithDB
	{
		/// <summary>
		/// DO NOT USE THIS UNLESS YOU KNOW WHAT YOU ARE DOING!
		/// 
		/// This method will safely delete a business object that has already been deleted in the database.
		/// 
		/// It will bypass business logic contracts (e.g. Unpicking Picked Pick Lines) and as such should only be used when the business object
		/// has already been validly deleted (i.e. when we are resyncing bizos already deleted in the database)
		/// 
		/// Does *NOT* verify the bizo has been deleted in the database.
		/// 
		/// </summary>
		void SafeDeleteForBizOAlreadyDeletedInDatabase_DoNotUse();
	}
}

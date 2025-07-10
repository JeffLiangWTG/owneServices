 _______      ________        _        ______       ____    ____   ________   _   _ 
|_   __ \    |_   __  |      / \      |_   _ `.    |_   \  /   _| |_   __  | | | | |
  | |__) |     | |_ \_|     / _ \       | | `. \     |   \/   |     | |_ \_| | | | |
  |  __ /      |  _| _     / ___ \      | |  | |     | |\  /| |     |  _| _  | | | |
 _| |  \ \_   _| |__/ |  _/ /   \ \_   _| |_.' /    _| |_\/_| |_   _| |__/ | |_| |_|
|____| |___| |________| |____| |____| |______.'    |_____||_____| |________| (_) (_)


IF YOU NEED TO UPDATE ONE OF THE FULL BACKUP FILES:
- restore the file manually with the DBBackupAndRestore tool (\dev\bin\Enterprise.DbBackupAndRestore.exe) or in SQL Server Management Studio (SSMS)
- apply your changes (new tables / columns / data setup  / etc.)
- do a full backup with the DBBackupAndRestore tool (\dev\bin\Enterprise.DbBackupAndRestore.exe) or in SSMS and overwrite your test file backup in TestFiles\

IF YOU NEED TO UPDATE ONE OF THE DIFF BACKUP FILES:
- apply your changes to the main DB this DIFF relates to
- do a differential backup in SSMS and overwrite your test file backup in TestFiles\. Use the INIT backup option, which you can set in SSMS from the Back Up Database window > Media Options (tab on the left) > Overwrite media > Overwrite all existing backup sets

# LicenceDatabaseSubscriberDataSchema Changelog
(See [example](../ExampleChangelog.md) for how to update this document)

## [v5] - 2024-09-06
### Added
- SpecificColumns
	- `LD_SystemCreateTimeUtc`
	- `LD_SystemCreateUser`
	- `LD_SystemLastEditTimeUtc`
	- `LD_SystemLastEditUser`

## [v4] - 2024-07-26
### Added
- SpecificColumns
	- `LD_TenantID`

## [v3] - 2024-04-03
### Removed
- Columns
	- `LD_TenantID`

## [v2] - 2024-02-14
- Columns
	- `LD_LicenceExpiry`
	- `LD_ReleaseRing`
	- `LD_OH_BillingParty`
	- `LD_Status`
	- `LD_LD_ParentDatabase`
	- `LD_GS_NKOwner`
	- `LD_TenantID`


## [v1] - 2023-12-14

### Initial schema

- Columns
	- `LD_PK`
	- `LD_IsActive`
	- `LD_LE`
	- `LD_LicenceType`
	- `LD_ServerCode`
	- `LD_DatabaseNumber`
	- `LD_Billable`
	- `LD_HostedLocation`
	- `LD_LastHeartbeat`
	- `LD_Product`
